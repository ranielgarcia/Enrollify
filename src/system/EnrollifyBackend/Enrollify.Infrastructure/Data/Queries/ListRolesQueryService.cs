using Enrollify.Application.Features.Roles.DTOs;
using Enrollify.Application.Features.Roles.Queries;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Infrastructure.Data.Queries;

internal class ListRolesQueryService : IListRolesQueryService
{
    private readonly EnrollifyDbContext _dbContext;

    public ListRolesQueryService(EnrollifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<RoleDto>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions)
            .Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionScopes = role.RolePermissions
                        .Select(ps => new RolePermissionDto
                        {
                            PermissionScope = PermissionScopeEnum.FromValue(ps.PermissionScopeId.Value),
                            Permissions = PermissionEnum.FromValue(ps.BitmaskPermission)
                        })
                        .ToList()
            }).ToListAsync();

        return roles;
    }
}
