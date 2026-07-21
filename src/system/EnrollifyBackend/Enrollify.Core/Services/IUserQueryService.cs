using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Services;

public interface IUserQueryService
{
  Task<List<UserIdRoleId>> GetUserIdsWithRoles(RoleId[] roleIds, CancellationToken ct);
  Task<List<UserId>> GetAllUserIds(CancellationToken ct);
}

public record UserIdRoleId(UserId UserId, RoleId RoleId);
