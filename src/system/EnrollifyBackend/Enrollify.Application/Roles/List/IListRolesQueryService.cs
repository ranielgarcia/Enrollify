namespace Enrollify.Application.Roles.List;

public interface IListRolesQueryService
{
    Task<List<RoleDTO>> ListRolesAsync(CancellationToken cancellationToken = default);
}
