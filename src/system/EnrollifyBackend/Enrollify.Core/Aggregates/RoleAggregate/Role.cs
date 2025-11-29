using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoleAggregate;

public class Role : EntityBase<Role, RoleId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private readonly List<RolePermission> _rolePermissions = new();
    private Role() { }// EF Core constructor

    public Role(RoleName name, RoleDescription description)
    {
        Name = name;
        Description = description;
    }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public RoleName Name { get; private set; }

    public RoleDescription Description { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();


    public Role UpdateName (RoleName newName)
    {
        if (Name == newName) return this;
        Name = newName;

        return this;
    }

    public Role UpdateDescription (RoleDescription newDescription)
    {
        if (Description == newDescription) return this;
        Description = newDescription;

        return this;
    }


    public Role AddPermission (PermissionId permissionId, UserId addedBy)
    {
        Guard.Against.Null(permissionId);
        Guard.Against.Null(addedBy);

        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return this;

        var rolePermission = new RolePermission(this.Id, permissionId, addedBy);
        rolePermission.AuditInfo.SetCreatedBy(addedBy);

        _rolePermissions.Add(rolePermission);

        return this;
    }

    public Role RemovePermission (PermissionId permissionId)
    {
        Guard.Against.Null(permissionId);
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
        return this;
    }
}
