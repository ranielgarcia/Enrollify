using Enrollify.Core.Aggregates.PermissionScopeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Core.Aggregates.RoleAggregate;

public class RolePermission : IAuditable<AuditInfo>
{
    public RolePermission() { } // EF Core constructor

    public RolePermission(RoleId roleId, PermissionScopeEnum permissionScopeId, PermissionEnum bitmaskPermission, UserId addedBy)
    {
        RoleId = roleId;
        PermissionScopeId = PermissionScopeId.From(permissionScopeId.Value);
        BitmaskPermission = bitmaskPermission;
        AuditInfo.SetCreatedBy(addedBy);
    }

    public RoleId RoleId { get; private set; }

    public PermissionScopeId PermissionScopeId { get; private set; }

    // We don't need to expose the PermissionScopes table in the code as it is directly mapped to PermissionScopeEnum
    public PermissionScopeEnum PermissionScope => PermissionScopeEnum.FromValue(PermissionScopeId.Value);

    public int BitmaskPermission { get; private set; }

    public IEnumerable<PermissionEnum> Permissions => PermissionEnum.FromValue(BitmaskPermission);

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();
}
