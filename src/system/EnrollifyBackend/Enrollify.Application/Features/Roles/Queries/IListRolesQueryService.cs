using Enrollify.Application.Features.Roles.DTOs;

namespace Enrollify.Application.Features.Roles.Queries;

public interface IListRolesQueryService
{
    Task<List<RoleDTO>> ListRolesAsync(CancellationToken cancellationToken = default);
}
