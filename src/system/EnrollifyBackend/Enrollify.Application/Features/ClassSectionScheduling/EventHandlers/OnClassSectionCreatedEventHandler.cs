using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public sealed class OnClassSectionCreatedEventHandler
{
  public RefreshClassSectionValidationIssuesRequestedEvent Handle
    (ClassSectionCreatedEvent notification)
  {
    return new RefreshClassSectionValidationIssuesRequestedEvent(notification.Id, notification.TriggeredBy);
  }
}
