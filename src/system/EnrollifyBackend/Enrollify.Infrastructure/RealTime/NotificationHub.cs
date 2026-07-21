using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Enrollify.Infrastructure.RealTime;

/// <summary>
/// Real-time hub for pushing notification events to authenticated clients.
/// The server only pushes ("ReceiveNotification") - there are currently no
/// client-invocable methods, so the hub body is intentionally empty.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
}
