using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;
using Enrollify.Core.Services.ScheduleConflictDetection;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;

public class ComputeAndGetDataIntegrityValidationIssuesForClassSection
{
  public sealed record Command(
    ClassSectionId ClassSectionId) : IRequest<Result<List<ClassSectionValidationIssueDto>>>;


  public sealed class Handler : IRequestHandler<Command, Result<List<ClassSectionValidationIssueDto>>>
  {
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly ClassSectionDataIntegrityValidator _dataIntegrityValidator;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionValidationIssueRepository _validationIssueRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      ClassSectionDataIntegrityValidator dataIntegrityValidator,
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionValidationIssueRepository validationIssueRepository,
      IPublisher publisher,
      ILogger<Handler> logger)
    {
      _offeringReadRepository = offeringReadRepository;
      _dataIntegrityValidator = dataIntegrityValidator;
      _classSectionReadRepository = classSectionReadRepository;
      _validationIssueRepository = validationIssueRepository;
      _publisher = publisher;
      _logger = logger;
    }

    public async Task<Result<List<ClassSectionValidationIssueDto>>> Handle(Command request,
      CancellationToken cancellationToken)
    {
      ClassSectionId classSectionId = request.ClassSectionId;

      ClassSection? section =
        await _classSectionReadRepository.FirstOrDefaultAsync(new GetClassSectionFullDetailsByIdSpec(classSectionId),
          cancellationToken);
      if (section is null)
      {
        _logger.LogWarning(
          "RefreshClassSectionValidationIssuesRequestedEventHandler: ClassSection {ClassSectionId} not found — skipping",
          classSectionId.Value);
        return Result.NotFound($"ClassSection with ID {classSectionId.Value} was not found.");
      }

      List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
        new GetClassSectionSubjectOfferingsByClassSectionIdSpec(section.Id), cancellationToken);

      List<ClassSectionDataIntegrityResult> dataIntegrityValidationResult =
        _dataIntegrityValidator.Validate(section, offerings);

      var dataIntegrityValidationIssues =
        dataIntegrityValidationResult.Select(r =>
          new ClassSectionValidationIssue(section.CourseId, section.AcademicTermId, section.Id,
            r)).ToList();

      var dataIntegrityValidationIssueTypes =
        ClassSectionValidationIssueTypeEnum.List.Where(x =>
          x.Tier == ClassSectionValidationIssueTierEnum.DATA_INTEGRITY);
      await _validationIssueRepository.ReplaceAllSpecificIssuesForSectionAsync(section.Id, dataIntegrityValidationIssues, dataIntegrityValidationIssueTypes, cancellationToken);

      await _publisher.Publish(
        new RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(section.AcademicTermId, section.CourseId,
          section.Id), cancellationToken);
      _logger.LogInformation(
        "Computed {DataIntegrityIssueCount} data integrity validation issues for class section {ClassSectionId}",
         dataIntegrityValidationIssues.Count, section.Id.Value);

      return Result.Success(dataIntegrityValidationIssues.Select(ClassSectionValidationIssueDto.FromEntity).ToList());
    }
  }
}
