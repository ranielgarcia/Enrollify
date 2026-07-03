using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionSchedulingStats;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public class RefreshClassSectionSchedulingStatsAggregateCountsRequestedEventHandler(
  IMediator mediator)
  : IDomainEventHandler<RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent>
{
  public async Task Handle(RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent notification,
    CancellationToken cancellationToken)
  {
    await mediator.Send(new ComputeClassSectionSchedulingStats.Command(
      notification.TermId,
      notification.CourseId,
      notification.ClassSectionId), cancellationToken);
  }
}
