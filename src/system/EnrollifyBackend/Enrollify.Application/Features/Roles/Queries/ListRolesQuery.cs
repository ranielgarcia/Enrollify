using Enrollify.Application.Features.Roles.DTOs;

namespace Enrollify.Application.Features.Roles.Queries;

public class ListRolesQuery : IRequest<Result<List<RoleDto>>>
{
}

public class ListRolesQueryHandler : IRequestHandler<ListRolesQuery, Result<List<RoleDto>>>
{
    private readonly IListRolesQueryService _queryService;

    public ListRolesQueryHandler(IListRolesQueryService queryService)
    {
        _queryService = queryService;
    }
    public async Task<Result<List<RoleDto>>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _queryService.ListRolesAsync(cancellationToken);
        return Result.Success(roles);
    }
}
