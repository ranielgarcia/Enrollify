using Dapper;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Services;
using Enrollify.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Enrollify.Infrastructure.Services;

public class UserQueryService : IUserQueryService
{
  private readonly IDbConnectionFactory _connectionFactory;

  public UserQueryService(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<List<UserIdRoleId>> GetUserIdsWithRoles(RoleId[] roleIds, CancellationToken ct)
  {
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = "SELECT UserId, RoleId FROM UserRolesAssignments WHERE RoleId IN @RoleIds";

    var command = new CommandDefinition(
      commandText: sql,
      parameters: new { RoleIds = roleIds.Select(r => r.Value) },
      cancellationToken: ct
    );
    IEnumerable<UserIdRoleId> userIds = await conn.QueryAsync<UserIdRoleId>(command);

    return  userIds.ToList();
  }

  public async Task<List<UserId>> GetAllUserIds(CancellationToken ct)
  {
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = "SELECT Id FROM Users WHERE isActive=1";

    var command = new CommandDefinition(
      commandText: sql,
      cancellationToken: ct
    );
    IEnumerable<UserId> userIds = await conn.QueryAsync<UserId>(command);

    return  userIds.ToList();
  }
}
