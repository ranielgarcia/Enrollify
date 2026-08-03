using Enrollify.Application.Features.ClassSectionScheduling.Events;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public sealed class OnClassSectionCreatedEventHandler
{
  public RefreshClassSectionValidationIssuesRequestedEvent Handle
    (ClassSectionCreatedEvent notification)
  {
    return new RefreshClassSectionValidationIssuesRequestedEvent(notification.Id, notification.TriggeredBy);
  }
}
