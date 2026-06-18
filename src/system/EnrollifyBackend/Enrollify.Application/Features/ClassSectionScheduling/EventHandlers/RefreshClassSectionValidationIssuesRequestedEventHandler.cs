using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

/// <summary>
/// Handles <see cref="RefreshClassSectionValidationIssuesRequestedEvent"/> — re-runs the full
/// eligibility validation pipeline and replaces all stored messages for the affected section.
/// Triggered by adviser updates on <see cref="ClassSection"/> and by teacher, room, or schedule
/// changes on <see cref="ClassSectionSubjectOffering"/>.
/// </summary>
public sealed class RefreshClassSectionValidationIssuesRequestedEventHandler(
  IReadRepository<ClassSection> classSectionReadRepository,
  IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
  IClassSectionValidationIssueRepository validationIssueRepository,
  ClassScheduleConflictDetector conflictDetector,
  ClassSectionDataIntegrityValidator dataIntegrityValidator,
  IClassSectionSubjectOfferingScheduleConflictRepository conflictRepo,
  IPublisher publisher,
  ILogger<RefreshClassSectionValidationIssuesRequestedEventHandler> logger)
  : IDomainEventHandler<RefreshClassSectionValidationIssuesRequestedEvent>
{
  public async Task Handle(RefreshClassSectionValidationIssuesRequestedEvent notification,
    CancellationToken cancellationToken)
  {
    ClassSectionId classSectionId = notification.ClassSectionId;

    ClassSection? section =
      await classSectionReadRepository.FirstOrDefaultAsync(new GetClassSectionFullDetailsByIdSpec(classSectionId),
        cancellationToken);
    if (section is null)
    {
      logger.LogWarning(
        "RefreshClassSectionValidationIssuesRequestedEventHandler: ClassSection {ClassSectionId} not found — skipping",
        classSectionId.Value);
      return;
    }

    List<ClassSectionSubjectOffering> offerings = await offeringReadRepository.ListAsync(
      new GetClassSectionSubjectOfferingsByClassSectionIdSpec(classSectionId), cancellationToken);

    List<ClassSectionDataIntegrityResult> dataIntegrityValidationResult =
      dataIntegrityValidator.Validate(section, offerings);

    var dataIntegrityValidationIssues =
      dataIntegrityValidationResult.Select(r =>
        new ClassSectionValidationIssue(section.Course!.CollegeId, section.CourseId, section.AcademicTermId, section.Id,
          r)).ToList();

    IEnumerable<TeacherId> teacherIds = offerings.Where(o => o.TeacherId != null).Select(o => o.TeacherId!.Value);
    IEnumerable<RoomId> roomIds = offerings.Where(o => o.RoomId != null).Select(o => o.RoomId!.Value);

    List<ClassScheduleConflictResult> conflictDetectionResults = new();
    // If no teachers or rooms assigned, only check section-level conflicts
    if (!teacherIds.Any() && !roomIds.Any())
    {
      // Still check for section overlap conflicts
      List<ClassScheduleConflictProjectionDto> sectionOnlySchedules =
        await conflictRepo.GetSectionSchedulesForConflictDetectionAsync(
          section.Id, cancellationToken);
      conflictDetectionResults = conflictDetector.DetectConflicts(sectionOnlySchedules, section.Id);
    }
    else
    {
      // Load this section's schedules
      List<ClassScheduleConflictProjectionDto> thisSectionSchedules =
        await conflictRepo.GetSectionSchedulesForConflictDetectionAsync(
          section.Id, cancellationToken);

      // Load related schedules for same teacher/room in same term (excluding this section)
      List<ClassScheduleConflictProjectionDto> relatedSchedules =
        await conflictRepo.GetRelatedSchedulesForConflictDetectionAsync(
          teacherIds, roomIds, section.AcademicTermId, section.Id, cancellationToken);

      // Combine and detect conflicts
      var allSchedules = thisSectionSchedules.Concat(relatedSchedules).ToList();
      conflictDetectionResults = conflictDetector.DetectConflicts(allSchedules, section.Id);
    }

    var conflictValidationIssues =
      conflictDetectionResults.Select(c =>
        new ClassSectionValidationIssue(section.Course!.CollegeId, section.CourseId, section.AcademicTermId, section.Id,
          c)).ToList();

    var validationIssues = conflictValidationIssues.Concat(dataIntegrityValidationIssues).ToList();

    await validationIssueRepository.ReplaceAllForSectionAsync(section.Id, validationIssues, cancellationToken);

    await publisher.Publish(
      new RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(section.AcademicTermId, section.CourseId,
        section.Id), cancellationToken);

    logger.LogInformation(
      "Eligibility recomputed for ClassSection {ClassSectionId}: {ErrorCount} error(s)",
      classSectionId.Value, validationIssues.Count);
  }
}
