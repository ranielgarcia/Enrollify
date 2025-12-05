using Enrollify.Core.Aggregates.PermissionScopeAggregate;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Core.Authentication;

public class UserContext
{
    public UserId Id { get; private set; }
    public UserEmail Email { get; private set; }
    public string FullName { get; private set; } = null!;
    public DateTimeOffset LastLoginAt { get; set; }

    public List<UserRoleContext> Roles { get; set; } = new List<UserRoleContext>();

    public static string Key => nameof(UserContext);
}

public class UserRoleContext
{
    public RoleId Id { get; set; }
    public RoleName Name { get; set; }
    public RoleDescription Description { get; set; }

    public List<RolePermissionContext> Permissions { get; set; } = new List<RolePermissionContext>();
}

public class RolePermissionContext
{
    public PermissionScopeId PermissionScopeId { get; set; }
    public string PermissionScopeName { get; set; } = null!;
    public PermissionEnum BitmaskPermission { get; set; } = PermissionEnum.None;
}