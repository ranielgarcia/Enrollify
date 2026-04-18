using Ardalis.Result;
using Enrollify.Application.Features.Roles.DTOs;
using Mediator;

namespace Enrollify.Application.Features.Roles.Features.List;

public class ListRolesQuery : IQuery<Result<List<RoleDTO>>>
{
}

public class ListRolesQueryHandler : IQueryHandler<ListRolesQuery, Result<List<RoleDTO>>>
{
    private readonly IListRolesQueryService _queryService;

    public ListRolesQueryHandler(IListRolesQueryService queryService)
    {
        _queryService = queryService;
    }
    public async ValueTask<Result<List<RoleDTO>>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _queryService.ListRolesAsync(cancellationToken);
        return Result.Success(roles);
    }
}