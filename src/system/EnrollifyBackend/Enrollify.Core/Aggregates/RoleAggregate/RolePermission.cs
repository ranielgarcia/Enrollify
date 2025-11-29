using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Aggregates.RoleAggregate;

public class RolePermission : IAuditable<AuditInfo>
{
    public RolePermission() { } // EF Core constructor

    public RolePermission(RoleId roleId, PermissionId permissionId, UserId addedBy)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        AuditInfo.SetCreatedBy(addedBy);
    }

    public RoleId RoleId { get; private set; }
    public PermissionId PermissionId { get; private set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();
}
