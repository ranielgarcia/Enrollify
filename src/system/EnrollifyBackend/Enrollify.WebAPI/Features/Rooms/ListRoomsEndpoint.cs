using Enrollify.Application.Features.Rooms.DTOs;
using Enrollify.Application.Features.Rooms.Queries;

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
