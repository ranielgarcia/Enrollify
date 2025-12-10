using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.Application.Roles.DTOs;

public class RoleDTO
{
    public RoleId Id { get; set; }
    public RoleName Name { get; set; }
    public RoleDescription Description { get; set; }
    public List<RolePermissionDTO> PermissionScopes { get; set; } = new List<RolePermissionDTO>();
}
