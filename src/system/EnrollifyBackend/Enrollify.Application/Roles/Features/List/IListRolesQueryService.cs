using Enrollify.Application.Roles.DTOs;

namespace Enrollify.Application.Roles.Features.List;

public interface IListRolesQueryService
{
    Task<List<RoleDTO>> ListRolesAsync(CancellationToken cancellationToken = default);
}
