using Enrollify.Application.Rooms.Features;
using Enrollify.Core.Aggregates.RoomAggregate;

namespace Enrollify.WebAPI.Features.Rooms;

public class DeleteRoomRequest
{
    [QueryParam]
    public int Id { get; set; }
}

public class DeleteRoomRequestValidator : Validator<DeleteRoomRequest>
{
    public DeleteRoomRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid building ID.");
    }
}

[HttpDelete("")]
[Group<RoomsEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteRoomPermission)]
public class DeleteEndpoint : Endpoint<DeleteRoomRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public DeleteEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult>
        ExecuteAsync(DeleteRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRoom.Command(RoomId.From(request.Id)), cancellationToken);
        return result.ToDeleteResult();
    }
}
