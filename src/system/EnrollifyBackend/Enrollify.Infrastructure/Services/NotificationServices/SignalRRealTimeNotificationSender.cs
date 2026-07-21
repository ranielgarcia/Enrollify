using Enrollify.Application.Features.Notifications;
using Enrollify.Application.Features.Notifications.DTOs;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Infrastructure.RealTime;
using Microsoft.AspNetCore.SignalR;

namespace Enrollify.Infrastructure.Services.NotificationServices;

public class SignalRRealTimeNotificationSender : IRealTimeNotificationSender
{
  private readonly IHubContext<NotificationHub> _hubContext;

  public SignalRRealTimeNotificationSender(IHubContext<NotificationHub> hubContext)
  {
    _hubContext = hubContext;
  }

  public async Task SendToUsersAsync(IEnumerable<UserId> userIds, NotificationDto notification, CancellationToken ct)
  {
    var userIdValues = userIds.Select(id => id.Value.ToString()).ToArray();
    if (userIdValues.Length == 0)
      return;

    await _hubContext.Clients.Users(userIdValues).SendAsync("ReceiveNotification", notification, ct);
  }
}
