using Enrollify.Core.Authentication;
using Microsoft.AspNetCore.SignalR;

namespace Enrollify.Infrastructure.RealTime;

/// <summary>
/// Maps SignalR connections to our internal <see cref="UserId"/> so notifications can be
/// targeted with <c>Clients.User(...)</c>/<c>Clients.Users(...)</c>. Relies on the
/// <see cref="UserContext"/> already resolved per-request by UserContextMiddleware and stashed
/// in <see cref="HttpContext.Items"/>, keeping this in sync with how the rest of the app
/// resolves "who is the current user".
/// </summary>
public class SignalRUserIdProvider : IUserIdProvider
{
  public string? GetUserId(HubConnectionContext connection)
  {
    var userContext = connection.GetHttpContext()?.Items[UserContext.Key] as UserContext;
    return userContext?.Id.Value.ToString();
  }
}
