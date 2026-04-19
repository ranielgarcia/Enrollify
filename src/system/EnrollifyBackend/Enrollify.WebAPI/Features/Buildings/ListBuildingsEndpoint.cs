using Enrollify.Application.Features.Buildings.Queries;
using Enrollify.Application.Features.Buildings.DTOs;

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
