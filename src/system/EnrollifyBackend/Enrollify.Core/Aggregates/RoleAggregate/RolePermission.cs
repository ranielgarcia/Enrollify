using Enrollify.Core.Aggregates.PermissionScopeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Core.Aggregates.RoleAggregate;

public class RolePermission : IAuditable<AuditInfo>
{
    public RolePermission() { } // EF Core constructor

    public RolePermission(RoleId roleId, PermissionScopeId permissionScopeId, PermissionEnum bitmaskPermission, UserId addedBy)
    {
        RoleId = roleId;
        PermissionScopeId = permissionScopeId;
        BitmaskPermission = bitmaskPermission;
        AuditInfo.SetCreatedBy(addedBy);
    }

    public RoleId RoleId { get; private set; }

    public PermissionScopeId PermissionScopeId { get; private set; }

    public PermissionEnum BitmaskPermission { get; private set; } = PermissionEnum.None;

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();
}
