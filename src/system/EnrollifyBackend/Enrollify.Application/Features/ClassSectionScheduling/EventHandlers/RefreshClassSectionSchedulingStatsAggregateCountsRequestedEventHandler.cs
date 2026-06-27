using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;

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

      Task refreshScheduleConflictIssueCountsForClassSectionTask =
        statsRepository.RefreshScheduleConflictIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshSchedulePolicyViolationIssueCountsForClassSectionTask =
        statsRepository.RefreshSchedulePolicyViolationIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshCapacityConstraintIssueCountsForClassSectionTask =
        statsRepository.RefreshCapacityConstraintIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshResourceMisalignmentIssueCountsForClassSectionTask =
        statsRepository.RefreshResourceMisalignmentIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshMissingRequirementIssueCountsForClassSectionTask =
        statsRepository.RefreshMissingRequirementIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshDataInconsistencyIssueCountsForClassSectionTask =
        statsRepository.RefreshDataInconsistencyIssueCountsForClassSection(notification.TermId,
          notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      Task refreshDefaultValueIssueCountsForClassSectionTask =
        statsRepository.RefreshDefaultValueIssueCountsForClassSection(notification.TermId,
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
        refreshScheduleConflictIssueCountsForClassSectionTask,
        refreshSchedulePolicyViolationIssueCountsForClassSectionTask,
        refreshCapacityConstraintIssueCountsForClassSectionTask,
        refreshResourceMisalignmentIssueCountsForClassSectionTask,
        refreshMissingRequirementIssueCountsForClassSectionTask,
        refreshDataInconsistencyIssueCountsForClassSectionTask,
        refreshDefaultValueIssueCountsForClassSectionTask,
        refreshOfferingCountWithIssueForClassSectionTask,
        refreshOfferingCountWithMissingTeacherIssueForClassSectionTask,
        refreshOfferingCountWithMissingRoomIssueForClassSectionTask,
        refreshOfferingCountWithNoScheduleIssueForClassSectionTask);
    }
  }
}
