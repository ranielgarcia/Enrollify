using Ardalis.Result;
using Enrollify.Application.Features.Roles.DTOs;
using Mediator;

namespace Enrollify.Application.Features.Roles.Queries;

public class ListRolesQuery : IQuery<Result<List<RoleDto>>>
{
}

public class ListRolesQueryHandler : IQueryHandler<ListRolesQuery, Result<List<RoleDto>>>
{
    private readonly IListRolesQueryService _queryService;

    public ListRolesQueryHandler(IListRolesQueryService queryService)
    {
        _queryService = queryService;
    }
    public async ValueTask<Result<List<RoleDto>>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _queryService.ListRolesAsync(cancellationToken);
        return Result.Success(roles);
    }
}
