using Enrollify.Application.Roles;
using Enrollify.Application.Roles.DTOs;
using Enrollify.Application.Roles.Features.List;
using Enrollify.Core.Constants.Authorization;

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
                            PermissionScope = PermissionScopeEnum.FromValue(ps.PermissionScopeId.Value),
                            Permissions = PermissionEnum.FromValue(ps.BitmaskPermission)
                        })
                        .ToList()
            }).ToListAsync();

        return roles;
    }
}
