using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Services.ClientDataInvalidation;

public interface IClientDataInvalidationDispatcher
{
  Task BroadcastClientDataInvalidation(string type, CancellationToken ct);
  Task DispatchClientDataInvalidationToTargetRole(string type, RoleId[] targetRoleIds, CancellationToken ct);
  Task DispatchClientDataInvalidationToTargetUser(string type, UserId[]? targetUserIds, CancellationToken ct);
}
