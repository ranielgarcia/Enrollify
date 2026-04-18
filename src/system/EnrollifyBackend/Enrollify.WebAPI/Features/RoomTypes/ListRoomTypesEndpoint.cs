using Enrollify.Application.Features.RoomTypes.DTOs;
using Enrollify.Application.Features.RoomTypes.Features;

namespace Enrollify.WebAPI.Features.RoomTypes;

[HttpGet("")]
[Group<RoomTypeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasViewRoomTypesPermission)]
public class ListRoomTypesEndpoint : EndpointWithoutRequest<List<RoomTypeDTO>>
{
    private readonly IMediator _mediator;

    public ListRoomTypesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await _mediator.Send(new ListRoomTypesQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
