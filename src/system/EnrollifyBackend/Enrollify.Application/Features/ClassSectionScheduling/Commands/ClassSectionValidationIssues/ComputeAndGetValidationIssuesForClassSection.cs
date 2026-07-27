using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Authentication;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;
using Enrollify.Core.Services.ScheduleConflictDetection;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;

public static class ComputeAndGetValidationIssuesForClassSection
{
  public sealed record Command(
    ClassSectionId ClassSectionId,
    UserId? TriggeredBy = null) : IRequest<Result<List<ClassSectionValidationIssue>>>;

  public sealed class Handler : IRequestHandler<Command, Result<List<ClassSectionValidationIssue>>>
  {
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly ClassScheduleConflictDetector _conflictDetector;
    private readonly ClassSectionDataIntegrityValidator _dataIntegrityValidator;
    private readonly IClassSectionSubjectOfferingScheduleConflictRepository _conflictRepo;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IClassSectionValidationIssueRepository _validationIssueRepository;
    private readonly IDomainEventBus _eventBus;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      ClassScheduleConflictDetector conflictDetector,
      ClassSectionDataIntegrityValidator dataIntegrityValidator,
      IClassSectionSubjectOfferingScheduleConflictRepository conflictRepo,
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IClassSectionValidationIssueRepository validationIssueRepository,
      IDomainEventBus eventBus,
      ICurrentUserAccessor currentUserAccessor,
      ILogger<Handler> logger)
    {
      _offeringReadRepository = offeringReadRepository;
      _conflictDetector = conflictDetector;
      _dataIntegrityValidator = dataIntegrityValidator;
      _conflictRepo = conflictRepo;
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _validationIssueRepository = validationIssueRepository;
      _eventBus = eventBus;
      _currentUserAccessor = currentUserAccessor;
      _logger = logger;
    }

    public async Task<Result<List<ClassSectionValidationIssue>>> Handle(Command request,
      CancellationToken cancellationToken)
    {
      ClassSectionId classSectionId = request.ClassSectionId;

      // Resolve the effective user: prefer the propagated TriggeredBy (background path),
      // fall back to the live HTTP context (direct-call path).
      UserId? effectiveUserId = request.TriggeredBy ?? _currentUserAccessor.GetCurrentUser()?.Id;

      ClassSection? section =
        await _classSectionReadRepository.FirstOrDefaultAsync(new GetClassSectionFullDetailsByIdSpec(classSectionId),
          cancellationToken);
      if (section is null)
      {
        _logger.LogWarning(
          "OnRefreshClassSectionValidationIssuesRequestedEventHandler: ClassSection {ClassSectionId} not found — skipping",
          classSectionId.Value);
        return Result.NotFound($"ClassSection with ID {classSectionId.Value} was not found.");
      }

      section.MoveToValidating(effectiveUserId);
      await _classSectionRepository.Update(section, cancellationToken);

      List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
        new GetClassSectionSubjectOfferingsByClassSectionIdSpec(section.Id), cancellationToken);

      List<ClassSectionDataIntegrityResult> dataIntegrityValidationResult =
        _dataIntegrityValidator.Validate(section, offerings);

      var dataIntegrityValidationIssues =
        dataIntegrityValidationResult.Select(r =>
          new ClassSectionValidationIssue(section.CourseId, section.AcademicTermId, section.Id,
            r)).ToList();

      IEnumerable<TeacherId> teacherIds = offerings.Where(o => o.TeacherId != null).Select(o => o.TeacherId!.Value);
      IEnumerable<RoomId> roomIds = offerings.Where(o => o.RoomId != null).Select(o => o.RoomId!.Value);

      List<ClassScheduleConflictResult> conflictDetectionResults = new();
      // If no teachers or rooms assigned, only check section-level conflicts
      if (!teacherIds.Any() && !roomIds.Any())
      {
        // Still check for section overlap conflicts
        List<ClassScheduleConflictProjectionDto> sectionOnlySchedules =
          await _conflictRepo.GetSectionSchedulesForConflictDetectionAsync(
            section.Id, cancellationToken);
        conflictDetectionResults = _conflictDetector.DetectConflicts(sectionOnlySchedules, section.Id);
      }
      else
      {
        // Load this section's schedules
        List<ClassScheduleConflictProjectionDto> thisSectionSchedules =
          await _conflictRepo.GetSectionSchedulesForConflictDetectionAsync(
            section.Id, cancellationToken);

        // Load related schedules for same teacher/room in same term (excluding this section)
        List<ClassScheduleConflictProjectionDto> relatedSchedules =
          await _conflictRepo.GetRelatedSchedulesForConflictDetectionAsync(
            teacherIds, roomIds, section.AcademicTermId, section.Id, cancellationToken);

        // Combine and detect conflicts
        var allSchedules = thisSectionSchedules.Concat(relatedSchedules).ToList();
        conflictDetectionResults = _conflictDetector.DetectConflicts(allSchedules, section.Id);
      }

      var conflictValidationIssues =
        conflictDetectionResults.Select(c =>
          new ClassSectionValidationIssue(section.CourseId, section.AcademicTermId, section.Id,
            c)).ToList();

      var validationIssues = conflictValidationIssues.Concat(dataIntegrityValidationIssues).ToList();

      await _validationIssueRepository.ReplaceAllForSectionAsync(section.Id, validationIssues, cancellationToken);


      await _eventBus.PublishAsync(
        new RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(section.AcademicTermId, section.CourseId,
          section.Id, triggeredBy: effectiveUserId));

      _logger.LogInformation(
        "Computed {ConflictIssueCount} conflict validation issues and {DataIntegrityIssueCount} data integrity validation issues for class section {ClassSectionId}",
        conflictValidationIssues.Count, dataIntegrityValidationIssues.Count, section.Id.Value);

      return Result.Success(validationIssues);
    }
  }
}
