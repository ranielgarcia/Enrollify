using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;

public static class BulkOpenClassSectionsForEnrollment
{
  public sealed record Command(List<ClassSectionId> Ids) : IRequest<Result<Unit>>;

  public sealed class Handler : IRequestHandler<Command, Result<Unit>>
  {
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IClassSectionSubjectOfferingRepository _classSectionSubjectOfferingRepository;
    private readonly IMediator _mediator;

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IClassSectionSubjectOfferingRepository classSectionSubjectOfferingRepository,
      IMediator mediator)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _offeringReadRepository = offeringReadRepository;
      _classSectionSubjectOfferingRepository = classSectionSubjectOfferingRepository;
      _mediator = mediator;
    }

    public async Task<Result<Unit>> Handle(Command command, CancellationToken cancellationToken)
    {
      List<ClassSection> sections = await _classSectionReadRepository.ListAsync(
        new GetClassSectionsByIdSpec(command.Ids), cancellationToken);

      var missingClassSections = sections.Where(s => !command.Ids.Contains(s.Id)).Select(s => s.Id).ToList();
      if (missingClassSections.Any())
        return Result.NotFound(
          $"Class sections with IDs {string.Join(", ", missingClassSections.Select(id => id.Value))} not found.");

      Dictionary<ClassSectionId, int> subjectOfferingsCountByClassSection =
        await _classSectionSubjectOfferingRepository.GetSubjectOfferingsCountPerClassSection(
          sections.Select(s => s.Id).ToList(), cancellationToken);

      foreach (ClassSection classSection in sections)
      {
        int offeringsCount =
          subjectOfferingsCountByClassSection.TryGetValue(classSection.Id, out int count) ? count : 0;

        if (offeringsCount == 0)
          return Result.Invalid(new ValidationError("EnrollmentEligibilityCheckFailed",
            $"Cannot open a {classSection.FullName} class section for enrollment with no subject offerings."));

        classSection.OpenForEnrollment();
      }

      Result result = await _classSectionRepository.BulkUpdate(sections, cancellationToken);

      return result.IsSuccess
        ? Result.Success(Unit.Value)
        : Result.Error(string.Join("; ", result.Errors));
    }
  }
}
