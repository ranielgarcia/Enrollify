using Enrollify.Application.Features.Notifications.Queries;

namespace Enrollify.WebAPI.Features.Notifications;

[HttpGet("unread-count")]
[Group<NotificationEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewNotificationPermission)]
public class GetUnreadNotificationsCountForUserEndpoint (IMediator mediator)
  : EndpointWithoutRequest<int>
{
  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await mediator.Send(new GetUnreadNotificationsCountForTheUserQuery(), ct);
    await Send.OkAsync(result, ct);
  }
}
