using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.PermissionScopeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants.Authorization;
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

    public Role AddPermission (PermissionScopeEnum permissionScopeId, PermissionEnum bitmaskPermission, UserId addedBy)
    {
        Guard.Against.Null(permissionScopeId);
        Guard.Against.Null(addedBy);

        if (_rolePermissions.Any(rp => rp.PermissionScopeId == permissionScopeId))
            return this;

        var rolePermission = new RolePermission(this.Id,  permissionScopeId, bitmaskPermission, addedBy);
        rolePermission.AuditInfo.SetCreatedBy(addedBy);

        _rolePermissions.Add(rolePermission);

        return this;
    }

    public Role RemovePermission (PermissionScopeId permissionScopeId)
    {
        Guard.Against.Null(permissionScopeId);
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionScopeId == permissionScopeId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
        return this;
    }

    public IEnumerable<(PermissionScopeId, int)> GetActivePermissions()
    {
        return _rolePermissions
            .Where(p => p.AuditInfo.IsActive)
            .Select(p => (p.PermissionScopeId, p.BitmaskPermission));
    }

    public bool HasPermission(PermissionScopeId permissionScopeId, PermissionEnum bitmaskPermission)
    {
        return _rolePermissions.Any(rp => 
            rp.PermissionScopeId == permissionScopeId &&
            rp.BitmaskPermission == bitmaskPermission && 
            rp.AuditInfo.IsActive);
    }
}
