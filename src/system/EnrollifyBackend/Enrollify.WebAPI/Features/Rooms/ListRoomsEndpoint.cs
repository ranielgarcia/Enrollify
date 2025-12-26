using Enrollify.Application.Rooms.DTOs;
using Enrollify.Application.Rooms.Features;

namespace Enrollify.WebAPI.Features.Rooms;

[HttpGet("")]
[Group<RoomsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewRoomsPermission)]
public class ListRoomsEndpoint (IMediator mediator) : EndpointWithoutRequest<List<RoomDTO>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListRoomsQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
