using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.Application.Features.Roles.DTOs;

public class RoleDto : BaseDto
{
    public RoleId Id { get; set; }
    public RoleName Name { get; set; }
    public RoleDescription Description { get; set; }
    public List<RolePermissionDto> PermissionScopes { get; set; } = new List<RolePermissionDto>();

}
