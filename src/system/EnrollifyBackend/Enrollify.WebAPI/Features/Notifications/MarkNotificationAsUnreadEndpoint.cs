using Enrollify.Application.Features.Notifications.Commands;
using Enrollify.Core.Aggregates.NotificationAggregate;

namespace Enrollify.WebAPI.Features.Notifications;

[HttpPut("{id:int}/unread")]
[Group<NotificationEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateNotificationPermission)]
public class MarkNotificationAsUnreadEndpoint(IMediator mediator)
  : EndpointWithoutRequest<OkOrNotFoundApiResult<int>>
{
  public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(CancellationToken ct)
  {
    int notificationId = Route<int>("id");
    var result = await mediator.Send(new MarkNotificationAsReadOrUnreadOrDismissed.Command(NotificationId.From(notificationId), MarkNotificationAsReadOrUnreadOrDismissed.Action.MarkUnread), ct);
    return result.ToUpdatedResult(id => id.Value);
  }
}
