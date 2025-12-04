using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Services.Authentication;
using Enrollify.Infrastructure.Data;
using Dapper;
using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.Infrastructure.Services.Authentication;

public class UserContextService : IUserContextService
{
    private readonly EnrollifyDbContext _db;
    private readonly IDbConnectionFactory _connectionFactory;

    public UserContextService(EnrollifyDbContext db, IDbConnectionFactory connectionFactory)
    {
        _db = db;
        _connectionFactory = connectionFactory;
    }

    public async ValueTask<UserContext?> GetUserContextByEmail(UserEmail email, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        using (var conn = await _connectionFactory.CreateOpenAsync(cancellationToken))
        {
            var query = @"SELECT U.Id, U.FirstName, U.LastName, U.Email, U.LastLoginAt, R.Id, R.Name, P.Name, P.Resource, P.Action, P.Description
                FROM Users U
                LEFT JOIN UserRolesAssignments URA ON URA.UserId=U.Id
                LEFT JOIN Roles R ON R.Id = URA.RoleId
                LEFT JOIN RolePermissions RP ON RP.RoleId = R.Id
                LEFT JOIN Permissions P ON P.Id = RP.PermissionId";


            var roles = await conn.QueryAsync<UserContext, UserRoleContext, UserRolePermissionContext, UserContext>(query,
                (user, role, permission) =>
                {
                    if (permission != null && !role.Permissions.Any(p => p.Id == permission.Id))
                    {
                        role.Permissions.Add(permission);
                    }

                    if (role != null && !user.Roles.Any(r => r.Id == role.Id))
                    {
                        user.Roles.Add(role);
                    }
                    
                    return user;
                });
        }

        // Single roundtrip:
        // - start from Users
        // - traverse owned UserRoleAssignments (left)
        // - join Roles (left)
        // - traverse owned RolePermissions (left)
        // - join Permissions (left)
        var rows = await (
            from u in _db.Users
            where u.Email == email
            from ua in u.RoleAssignments.DefaultIfEmpty()
                // Filter active (non-expired) role assignments; keep null to preserve user with no roles
            where ua == null || !ua.ExpiresAt.HasValue || ua.ExpiresAt.Value > now
            from r in _db.Roles.Where(rr => ua != null && rr.Id == ua.RoleId).DefaultIfEmpty()
            from rp in (r != null ? r.RolePermissions : Enumerable.Empty<Core.Aggregates.RoleAggregate.RolePermission>()).DefaultIfEmpty()
                // Filter active role-permissions; keep nulls
            where rp == null || rp.AuditInfo.IsActive
            from p in _db.Permissions.Where(pp => rp != null && pp.Id == rp.PermissionId).DefaultIfEmpty()
            select new { User = u, Role = r, Permission = p }
        )
        .AsNoTracking()
        .ToListAsync(cancellationToken);




        if (rows.Count == 0)
        {
            // User not found
            return null;
        }

        var user = rows[0].User;
        var context = UserContext.FromUser(user);

        // Group by role; null roles are skipped (user may have zero roles)
        foreach (var group in rows.Where(x => x.Role != null).GroupBy(x => x.Role!.Id))
        {
            var role = group.First().Role!;
            var permissions = group
                .Where(x => x.Permission != null)
                .Select(x => x.Permission!)
                .GroupBy(p => p.Id)        // de-dup
                .Select(g => g.First())
                .ToList();

            context.WithRoleAndPermissions(role, permissions);
        }

        return context;
    }
}