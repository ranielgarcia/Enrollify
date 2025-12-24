using Enrollify.Application.Buildings.DTOs;
using Enrollify.Application.Buildings.Features;
using Enrollify.WebAPI.Authorization;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Features.Buildings;

[HttpGet("")]
[Group<BuildingEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewBuildingPermission)]
public class ListBuildingsEndpoint(IMediator mediator) : EndpointWithoutRequest<List<BuildingDTO>>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListBuildingsQuery(), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
