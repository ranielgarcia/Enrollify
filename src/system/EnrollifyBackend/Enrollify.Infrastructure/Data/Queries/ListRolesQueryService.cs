using Enrollify.Application.Roles;
using Enrollify.Application.Roles.List;

namespace Enrollify.Infrastructure.Data.Queries;

internal class ListRolesQueryService : IListRolesQueryService
{
    private readonly EnrollifyDbContext _dbContext;

    public ListRolesQueryService(EnrollifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<RoleDTO>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions).Select(role => new RoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionScopes = role.RolePermissions
                        .Select(ps => new RolePermissionDTO
                        {
                            PermissionScopeId = ps.PermissionScopeId,
                            BitmaskPermission = ps.BitmaskPermission
                        })
                        .ToList()
            }).ToListAsync();

        return roles;
    }
}
