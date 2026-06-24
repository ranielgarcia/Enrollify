using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;
using Enrollify.SharedKernel;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public class RefreshClassSectionSchedulingStatsAggregateCountsRequestedEventHandler(
  IClassSectionSchedulingStatsRepository statsRepository)
  : IDomainEventHandler<RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent>
{
  public async Task Handle(RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent notification,
    CancellationToken cancellationToken)
  {
    Task refreshDraftSectionCountsForCourseTask = statsRepository.RefreshDraftSectionCountsForCourse(
      notification.TermId,
      notification.CourseId, cancellationToken);

    Task refreshOpenSectionCountsForCourseTask = statsRepository.RefreshOpenSectionCountsForCourse(notification.TermId,
      notification.CourseId, cancellationToken);

    Task refreshCancelledSectionCountsForCourseTask = statsRepository.RefreshCancelledSectionCountsForCourse(
      notification.TermId,
      notification.CourseId, cancellationToken);

    await Task.WhenAll(
      refreshDraftSectionCountsForCourseTask,
      refreshOpenSectionCountsForCourseTask,
      refreshCancelledSectionCountsForCourseTask);

    if (notification.ClassSectionId is not null)
    {
      Task refreshOfferingsCountForClassSectionTask =
        statsRepository.RefreshOfferingsCountForClassSection((ClassSectionId)notification.ClassSectionId,
          cancellationToken);

      Task refreshHardConflictIssueCountsForClassSectionTask =
        statsRepository.RefreshHardConflictIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshSoftConflictIssueCountsForClassSectionTask =
        statsRepository.RefreshSoftConflictIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshDataIntegrityIssueCountsForClassSectionTask =
        statsRepository.RefreshDataIntegrityIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshInformationalIssueCountsForClassSectionTask =
        statsRepository.RefreshInformationalIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshOfferingCountWithIssueForClassSectionTask =
        statsRepository.RefreshOfferingCountWithIssueForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshOfferingCountWithMissingTeacherIssueForClassSectionTask =
        statsRepository.RefreshOfferingCountWithMissingTeacherIssueForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshOfferingCountWithMissingRoomIssueForClassSectionTask =
        statsRepository.RefreshOfferingCountWithMissingRoomIssueForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshOfferingCountWithNoScheduleIssueForClassSectionTask =
        statsRepository.RefreshOfferingCountWithNoScheduleIssueForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await Task.WhenAll(
        refreshOfferingsCountForClassSectionTask,
        refreshHardConflictIssueCountsForClassSectionTask,
        refreshSoftConflictIssueCountsForClassSectionTask,
        refreshDataIntegrityIssueCountsForClassSectionTask,
        refreshInformationalIssueCountsForClassSectionTask,
        refreshOfferingCountWithIssueForClassSectionTask,
        refreshOfferingCountWithMissingTeacherIssueForClassSectionTask,
        refreshOfferingCountWithMissingRoomIssueForClassSectionTask,
        refreshOfferingCountWithNoScheduleIssueForClassSectionTask);
    }
  }
}
