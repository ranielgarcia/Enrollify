using Enrollify.Core.Aggregates.PermissionScopeAggregate;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Application.Roles;

public class RolePermissionDTO
{
    public PermissionScopeId PermissionScopeId { get; set; }
    public int BitmaskPermission { get; set; }
    public IEnumerable<PermissionEnum> Permissions => PermissionEnum.FromValue(BitmaskPermission);
}
