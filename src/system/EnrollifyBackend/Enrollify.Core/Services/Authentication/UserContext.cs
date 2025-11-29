using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Services.Authentication;

public class UserContext
{
    public UserId Id { get; private set; }
    public UserEmail Email { get; private set; }
    public string FullName { get; private set; } = null!;

    public List<UserRoleContext> Roles { get; set; } = new List<UserRoleContext>();

    public static string Key => nameof(UserContext);


    public static UserContext FromUser(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return new UserContext
        {
            Id = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}"
        };
    }

    public UserContext WithRoleAndPermissions(Role role, IEnumerable<Permission> permissions)
    {
        var rolePermissions = permissions.Select(p => new UserRolePermissionContext(p.Id, p.Name, p.Description,p.Action, p.Resource));
        Roles.Add(new UserRoleContext(role.Id, role.Name, role.Description, rolePermissions));
        return this;
    }
}

public record UserRoleContext(
    RoleId Id, 
    RoleName Name, 
    RoleDescription Description, 
    IEnumerable<UserRolePermissionContext> Permissions);
public record UserRolePermissionContext (
    PermissionId Id, 
    PermissionName Name, 
    PermissionDescription Description, 
    PermissionAction Action, 
    PermissionResource Resource);

