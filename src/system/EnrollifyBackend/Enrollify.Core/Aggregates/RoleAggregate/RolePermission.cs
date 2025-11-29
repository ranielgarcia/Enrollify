using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoleAggregate;

public class RolePermission : EntityBase<RolePermission, RolePermissionId>, IAuditable<AuditInfo>
{
    public RolePermission() { } // EF Core constructor

    public RolePermission(RoleId roleId, PermissionId permissionId, UserId addedBy)
    {
        RoleId = RoleId;
        PermissionId = permissionId;
    }

    public RoleId RoleId { get; private set; }
    public PermissionId PermissionId { get; private set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();
}
