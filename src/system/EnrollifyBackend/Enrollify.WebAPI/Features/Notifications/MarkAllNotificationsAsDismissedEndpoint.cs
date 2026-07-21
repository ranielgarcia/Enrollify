using Enrollify.Application.Features.Notifications.Commands;

namespace Enrollify.WebAPI.Features.Notifications;

[HttpPost("dismiss-all")]
[Group<NotificationEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateNotificationPermission)]
public class MarkAllNotificationsAsDismissedEndpoint (IMediator mediator) :
  EndpointWithoutRequest<OkOrNotFoundApiResult<Unit>>
{
  public override async Task<OkOrNotFoundApiResult<Unit>> ExecuteAsync(CancellationToken ct)
  {
    var result = await mediator.Send(new MarkAllNotificationsAsDismissed.Command(), ct);
    return result.ToUpdatedResult(_ => Unit.Value);
  }
}
