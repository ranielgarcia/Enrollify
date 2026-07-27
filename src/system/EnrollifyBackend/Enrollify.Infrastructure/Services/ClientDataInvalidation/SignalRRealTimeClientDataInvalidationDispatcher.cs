using Enrollify.Application.Features.ClientDataInvalidations;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Infrastructure.RealTime;
using Microsoft.AspNetCore.SignalR;

namespace Enrollify.Infrastructure.Services.ClientDataInvalidation;

public class SignalRRealTimeClientDataInvalidationDispatcher : IRealTimeClientDataInvalidationDispatcher
{
  private readonly IHubContext<ClientDataInvalidationHub> _hubContext;

  public SignalRRealTimeClientDataInvalidationDispatcher(IHubContext<ClientDataInvalidationHub> hubContext)
  {
    _hubContext = hubContext;
  }

  public async Task SendToUsersAsync(IEnumerable<UserId> userIds, string type, CancellationToken ct)
  {
    var userIdValues = userIds.Select(id => id.Value.ToString()).ToArray();
    if (userIdValues.Length == 0)
      return;

    await _hubContext.Clients.Users(userIdValues).SendAsync("ReceiveClientDataInvalidation", type, ct);
  }
}
