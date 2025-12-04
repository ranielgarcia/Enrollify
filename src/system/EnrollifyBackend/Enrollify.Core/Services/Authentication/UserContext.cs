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
        var rolePermissions = permissions
            .Select(p => new UserRolePermissionContext
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Action = p.Action,
                Resource = p.Resource,
            })
            .ToList();
        Roles.Add(new UserRoleContext
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = rolePermissions
        });
        return this;
    }
}

public class UserRoleContext
{
    public RoleId Id { get; set; }
    public RoleName Name { get; set; }
    public RoleDescription Description { get; set; }
    public List<UserRolePermissionContext> Permissions { get; set; } = new List<UserRolePermissionContext>();
}
public record UserRolePermissionContext()
{
    public PermissionId Id { get; set; }
    public PermissionName Name { get; set; }
    public PermissionDescription Description { get; set; }
    public PermissionAction Action { get; set; }
    public PermissionResource Resource { get; set; }
};

