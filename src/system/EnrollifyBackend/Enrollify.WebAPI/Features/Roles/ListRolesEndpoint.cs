using Enrollify.Application.Roles;
using Enrollify.Application.Roles.List;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Features.Roles;

[HttpGet("")]
[Group<RoleEndpointsGroup>]
[AllowAnonymous]
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
