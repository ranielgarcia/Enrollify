using Enrollify.Core.Aggregates.PermissionScopeAggregate;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Application.Features.Roles.DTOs;

public class RolePermissionDTO
{
    public PermissionScopeEnum PermissionScope { get; set; } = PermissionScopeEnum.None;
    public IEnumerable<PermissionEnum> Permissions { get; set; } = Enumerable.Empty<PermissionEnum>();
}
