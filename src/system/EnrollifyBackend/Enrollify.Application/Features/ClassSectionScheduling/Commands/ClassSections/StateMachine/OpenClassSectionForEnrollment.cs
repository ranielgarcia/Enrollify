using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;

public static class OpenClassSectionForEnrollment
{
  public sealed record Command(ClassSectionId Id) : IRequest<Result<ClassSectionId>>;

  public sealed class Handler : IRequestHandler<Command, Result<ClassSectionId>>
  {
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IClassSectionValidationIssueRepository _classSectionValidationIssueRepository;

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IClassSectionValidationIssueRepository classSectionValidationIssueRepository)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _offeringReadRepository = offeringReadRepository;
      _classSectionValidationIssueRepository = classSectionValidationIssueRepository;
    }

    public async Task<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
    {
      ClassSection? section = await _classSectionReadRepository.GetByIdAsync(command.Id, cancellationToken);
      if (section is null)
        return Result.NotFound($"Class section with ID {command.Id.Value} not found.");

      List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
        new GetClassSectionSubjectOfferingsByClassSectionIdSpec(command.Id), cancellationToken);

      if (offerings.Count == 0)
        return Result.Invalid(new ValidationError(
          "Cannot open a class section for enrollment with no subject offerings."));

      bool hasValidationErrors =
        await _classSectionValidationIssueRepository.HasValidationErrorsAsync(section.Id, cancellationToken);

      if (hasValidationErrors)
        return Result.Invalid(new ValidationError("This class section is not eligible for enrollment."));

      try
      {
        section.OpenForEnrollment();
        Result<ClassSectionId> result = await _classSectionRepository.Update(section, cancellationToken);

        return result.IsSuccess
          ? Result.Success(result.Value)
          : Result.Error(string.Join("; ", result.Errors));
      }
      catch (ArgumentException ex)
      {
        return Result.Invalid(new ValidationError(ex.Message));
      }
    }
  }
}
