using Enrollify.Application.Features.Rooms.Features;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.WebAPI.Features.RoomTypes;


[HttpGet("{roomTypeId}/rooms/count")]
[Group<RoomTypeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasViewRoomsPermission)]
public class CountAssociatedRoomsEndpoint (IMediator mediator) : EndpointWithoutRequest<int>
{
    public override async Task<int> HandleAsync(CancellationToken ct)
    {
        var roomTypeId = Route<int>("roomTypeId");
        var result = await mediator.Send(new CountRoomsByRoomTypeQuery { RoomTypeId = RoomTypeId.From(roomTypeId) }, ct);
        return result.IsSuccess ? result.Value : 0;
    }
}
