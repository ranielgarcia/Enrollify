using Enrollify.Application.Features.Notifications.DTOs;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Notifications;

/// <summary>
/// Pushes a notification to the currently-connected clients of the given users in real time
/// (e.g. via a SignalR hub), after it has already been persisted. Implemented in the
/// Infrastructure layer where the real-time transport lives.
/// </summary>
public interface IRealTimeNotificationSender
{
  Task SendToUsersAsync(IEnumerable<UserId> userIds, NotificationDto notification, CancellationToken ct);
}
