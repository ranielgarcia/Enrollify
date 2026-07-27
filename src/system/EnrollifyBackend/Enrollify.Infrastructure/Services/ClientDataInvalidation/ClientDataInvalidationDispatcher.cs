using Enrollify.Application.Features.ClientDataInvalidations;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Authentication;
using Enrollify.Core.Constants;
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
    List<UserId> allUserIds = await _userQueryService.GetAllUserIds(ct);
    await _realTimeClientDataInvalidationDispatcher.SendToUsersAsync(allUserIds, type, ct);
  }

  public async Task DispatchClientDataInvalidationToTargetRole(
    string type, RoleId[] targetRoleIds, CancellationToken ct)
  {
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
      targetUserIds = new[] { currentUser?.Id ?? SystemUserConstants.SystemUserId };
    }

    await _realTimeClientDataInvalidationDispatcher
      .SendToUsersAsync(targetUserIds, type, ct);
  }
}
