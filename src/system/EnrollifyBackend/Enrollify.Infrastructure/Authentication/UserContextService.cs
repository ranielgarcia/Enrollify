using Dapper;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Authentication;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Authentication;

public class UserContextService : IUserContextService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserContextService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask<UserContext?> GetUserContextByEmail(UserEmail email, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        using (var conn = await _connectionFactory.CreateOpenAsync(cancellationToken))
        {
            var query = @"
            SELECT 
                U.Id, CONCAT(U.FirstName, ' ', U.LastName) As FullName, U.Email, U.LastLoginAt,
                R.Id, R.Name, R.Description,
                RP.PermissionScopeId, PS.Name as PermissionScopeName, RP.BitmaskPermission
            FROM Users U
            LEFT JOIN UserRolesAssignments URA ON URA.UserId = U.Id 
                AND URA.IsActive = 1
                AND (URA.ExpiresAt IS NULL OR URA.ExpiresAt > @Now)
            LEFT JOIN Roles R ON R.Id = URA.RoleId AND R.IsActive = 1
            LEFT JOIN RolePermissions RP ON RP.RoleId = R.Id AND RP.IsActive = 1
            LEFT JOIN PermissionScopes PS ON PS.Id = RP.PermissionScopeId AND PS.IsActive = 1
            WHERE U.Email = @Email AND U.IsActive = 1
            ";

            UserContext? userContext = null;
            var roleLookup = new Dictionary<int, UserRoleContext>();

            await conn.QueryAsync<UserContext, UserRoleContext, RolePermissionScopeContext, UserContext>(
                query,
                (user, role, permissionScope) =>
                {
                    // Initialize user context only once
                    if (userContext == null)
                    {
                        userContext = user;
                    }

                    // Process role if present
                    if (role != null && role.Id.Value != 0)
                    {
                        // Check if role already exists in lookup
                        if (!roleLookup.TryGetValue(role.Id.Value, out var existingRole))
                        {
                            roleLookup.Add(role.Id.Value, role);
                            userContext.Roles.Add(role);
                            existingRole = role;
                        }

                        // Add permission to role if present and not already added
                        if (permissionScope != null && permissionScope.PermissionScopeId != 0)
                        {
                            if (!existingRole.PermissionScopes.Any(p => p.PermissionScopeId == permissionScope.PermissionScopeId))
                            {
                                existingRole.PermissionScopes.Add(permissionScope);
                            }
                        }
                    }

                    return userContext;
                },
                new { Email = email.Value, Now = now },
                splitOn: "Id,Id,PermissionScopeId"
            );

            return userContext;
        }
    }
}