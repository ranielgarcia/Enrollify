using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public sealed class OnClassSectionCreatedEventHandler(
  IDomainEventBus eventBus)
  : IDomainEventHandler<ClassSectionCreatedEvent>
{
  public async Task Handle(ClassSectionCreatedEvent notification, CancellationToken cancellationToken)
  {
    await eventBus.PublishAsync(new RefreshClassSectionValidationIssuesRequestedEvent(notification.Id));
  }
}
