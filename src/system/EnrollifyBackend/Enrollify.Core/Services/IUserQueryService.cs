using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Services;

public interface IUserQueryService
{
  Task<List<UserId>> GetUserIdsWithRole(RoleId roleId, CancellationToken ct);
  Task<List<UserId>> GetAllUserIds(CancellationToken ct);
}
