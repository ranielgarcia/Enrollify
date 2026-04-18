using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.Application.Features.Roles.DTOs;

public class RoleDTO : BaseDTO
{
    public RoleId Id { get; set; }
    public RoleName Name { get; set; }
    public RoleDescription Description { get; set; }
    public List<RolePermissionDTO> PermissionScopes { get; set; } = new List<RolePermissionDTO>();

}
