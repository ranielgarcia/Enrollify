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
    await statsRepository.RefreshDraftSectionCountsForCourse(notification.TermId,
      notification.CourseId, cancellationToken);

    await statsRepository.RefreshOpenSectionCountsForCourse(notification.TermId,
      notification.CourseId, cancellationToken);

    await statsRepository.RefreshCancelledSectionCountsForCourse(notification.TermId,
      notification.CourseId, cancellationToken);

    if (notification.ClassSectionId is not null)
    {
      await statsRepository.RefreshHardConflictIssueCountsForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshSoftConflictIssueCountsForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshDataIntegrityIssueCountsForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshInformationalIssueCountsForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshOfferingCountWithIssueForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshOfferingCountWithMissingTeacherIssueForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshOfferingCountWithMissingRoomIssueForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);

      await statsRepository.RefreshOfferingCountWithNoScheduleIssueForClassSection(notification.TermId,
        notification.CourseId, (ClassSectionId)notification.ClassSectionId, cancellationToken);
    }
  }
}
