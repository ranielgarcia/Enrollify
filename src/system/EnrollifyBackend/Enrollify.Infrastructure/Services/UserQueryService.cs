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

  public async Task<List<UserId>> GetUserIdsWithRole(RoleId roleId, CancellationToken ct)
  {
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = "SELECT UserId FROM UserRolesAssignments WHERE RoleId = @RoleId";

    var command = new CommandDefinition(
      commandText: sql,
      parameters: new { RoleId = roleId },
      cancellationToken: ct
    );
    IEnumerable<UserId> userIds = await conn.QueryAsync<UserId>(command);

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
