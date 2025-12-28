using Enrollify.Application.Roles.DTOs;
using Enrollify.Application.Roles.Features.List;

namespace Enrollify.WebAPI.Features.Roles;

[HttpGet("")]
[Group<RoleEndpointsGroup>]
[Authorize(Policy = PolicyName.HasViewRolesPermission)]
public class ListRolesEndpoint : EndpointWithoutRequest<List<RoleDTO>>
{
    private readonly IMediator _mediator;

    public ListRolesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await _mediator.Send(new ListRolesQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
