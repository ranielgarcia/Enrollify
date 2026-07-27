using Enrollify.Application.Features.ClientDataInvalidations;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Authentication;
using Enrollify.Core.Services;
using Enrollify.Core.Services.ClientDataInvalidation;

namespace Enrollify.Infrastructure.Services.ClientDataInvalidation;

public class ClientDataInvalidationDispatcher : IClientDataInvalidationDispatcher
{
  private readonly IUserQueryService _userQueryService;
  private readonly IRealTimeClientDataInvalidationDispatcher _realTimeClientDataInvalidationDispatcher;
  private readonly ICurrentUserAccessor _currentUserAccessor;

  public ClientDataInvalidationDispatcher
    (IUserQueryService userQueryService,
      IRealTimeClientDataInvalidationDispatcher  realTimeClientDataInvalidationDispatcher,
      ICurrentUserAccessor currentUserAccessor)
  {
    _userQueryService = userQueryService;
    _realTimeClientDataInvalidationDispatcher = realTimeClientDataInvalidationDispatcher;
    _currentUserAccessor = currentUserAccessor;
  }

  public async Task BroadcastClientDataInvalidation(string type, CancellationToken ct)
  {
    // TODO: Cache user ids
    List<UserId> allUserIds = await _userQueryService.GetAllUserIds(ct);
    await _realTimeClientDataInvalidationDispatcher.SendToUsersAsync(allUserIds, type, ct);
  }

  public async Task DispatchClientDataInvalidationToTargetRole(
    string type, RoleId[] targetRoleIds, CancellationToken ct)
  {
    // TODO: Cache user ids
    List<UserIdRoleId> userIdsWithRoles = await _userQueryService.GetUserIdsWithRoles(targetRoleIds, ct);
    UserId[] targetUserIds = userIdsWithRoles.Select(x => x.UserId).Distinct().ToArray();

    await _realTimeClientDataInvalidationDispatcher.SendToUsersAsync(targetUserIds, type, ct);
  }

  public async Task DispatchClientDataInvalidationToTargetUser(
    string type, UserId[]? targetUserIds, CancellationToken ct)
  {
    if (targetUserIds == null || targetUserIds.Length == 0)
    {
      var currentUser = _currentUserAccessor.GetCurrentUser();
      if (currentUser != null)
      {
        targetUserIds = new[] { currentUser.Id };
      }
      else
      {
        // No HTTP context (e.g. background Wolverine handler) and no TriggeredBy was propagated.
        // Broadcast to all connected users as a defensive fallback so the invalidation is not silently dropped.
        await BroadcastClientDataInvalidation(type, ct);
        return;
      }
    }

    await _realTimeClientDataInvalidationDispatcher
      .SendToUsersAsync(targetUserIds, type, ct);
  }
}
