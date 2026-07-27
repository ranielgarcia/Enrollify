using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Enrollify.Infrastructure.RealTime;

[Authorize]
public class ClientDataInvalidationHub : Hub
{

}
