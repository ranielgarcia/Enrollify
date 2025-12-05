using Dapper;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Services.Authentication;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Services.Authentication;

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
                    P.Id, P.Name, P.Resource, P.Action, P.Description
                FROM Users U
                LEFT JOIN UserRolesAssignments URA ON URA.UserId = U.Id 
                    AND URA.IsActive = 1
                    AND (URA.ExpiresAt IS NULL OR URA.ExpiresAt > @Now)
                LEFT JOIN Roles R ON R.Id = URA.RoleId AND R.IsActive = 1
                LEFT JOIN RolePermissions RP ON RP.RoleId = R.Id AND RP.IsActive = 1
                LEFT JOIN Permissions P ON P.Id = RP.PermissionId AND P.IsActive = 1
                WHERE U.Email = @Email AND U.IsActive = 1";

            UserContext? userContext = null;
            var roleLookup = new Dictionary<int, UserRoleContext>();

            await conn.QueryAsync<UserContext, UserRoleContext, UserRolePermissionContext, UserContext>(
                query,
                (user, role, permission) =>
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
                        if (permission != null && permission.Id.Value != 0)
                        {
                            if (!existingRole.Permissions.Any(p => p.Id.Value == permission.Id.Value))
                            {
                                existingRole.Permissions.Add(permission);
                            }
                        }
                    }

                    return userContext;
                },
                new { Email = email.Value, Now = now },
                splitOn: "Id,Id,Id"
            );

            return userContext;
        }
    }
}