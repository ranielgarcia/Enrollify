using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Subjects.Commands;

public static class DeleteSubject
{
  public sealed record Command(SubjectId id) : IRequest<Result>;

  public sealed class Handler : IRequestHandler<Command, Result>
  {
    private readonly ISubjectRepository _subjectRepository;
    private readonly IReadRepository<Curriculum> _curriculumReadRepository;
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;

    public Handler(
      ISubjectRepository subjectRepository,
      IReadRepository<Curriculum> curriculumReadRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository)
    {
      _subjectRepository = subjectRepository;
      _curriculumReadRepository = curriculumReadRepository;
      _offeringReadRepository = offeringReadRepository;
    }

    public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
    {
      List<Curriculum> curriculums =
        await _curriculumReadRepository.ListAsync(new GetCurriculumBySubjectIdSpec(command.id), cancellationToken);

      if (curriculums.Any())
      {
        string curriculumNames = string.Join(", ", curriculums.Select(c => $"{c.Description} ({c.Version})"));
        return Result.Forbidden(
          $"Cannot delete subject because it is associated with one or more curriculums. [{curriculumNames}]");
      }

      List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
        new GetClassSectionSubjectOfferingsBySubjectIdSpec(command.id), cancellationToken);
      if (offerings.Count > 0)
        return Result.Forbidden(
          $"Cannot delete subject — it is referenced in {offerings.Count} class section offering(s).");

      return await _subjectRepository.Delete(command.id, cancellationToken);
    }
  }
}
