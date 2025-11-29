using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Services.Authentication;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Services.Authentication;

public class UserContextService : IUserContextService
{
    private readonly EnrollifyDbContext _db;

    public UserContextService(EnrollifyDbContext db)
    {
        _db = db;
    }

    public async ValueTask<UserContext?> GetUserContextByEmail(UserEmail email, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

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