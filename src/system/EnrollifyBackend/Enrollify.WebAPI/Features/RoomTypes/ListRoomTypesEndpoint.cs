using Enrollify.Application.RoomTypes.DTOs;
using Enrollify.Application.RoomTypes.Features;
using Enrollify.WebAPI.Authorization;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Features.RoomTypes;

[HttpGet("")]
[Group<RoomTypeEndpointGroup>]
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
