using Enrollify.Application.Rooms.DTOs;
using Enrollify.Application.Rooms.Features;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.WebAPI.Authorization;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Features.Rooms;

public class ListByRoomTypeRequest
{
    [QueryParam]
    public int RoomTypeId { get; set; }

}

[HttpGet("list-by-room-type")]
[Group<RoomsEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewRoomsPermission)]
public class ListByRoomTypeEndpoint : Endpoint<ListByRoomTypeRequest, List<RoomDTO>>
{
    private readonly IMediator _mediator;

    public ListByRoomTypeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(ListByRoomTypeRequest req, CancellationToken ct)
    {
        var query = new ListByRoomTypeQuery { RoomTypeId = RoomTypeId.From(req.RoomTypeId) };
        var result = await _mediator.Send(query, ct);
        await Send.OkAsync(result);
    }

}
