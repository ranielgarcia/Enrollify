using Enrollify.Application.Features.Rooms.Commands;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.WebAPI.Features.Rooms;


public class UpdateRoomResponse
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomTypeId { get; set; }
    public int BuildingId { get; set; }
}

public class UpdateRoomRequest
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomTypeId { get; set; }
    public int BuildingId { get; set; }
}
public class UpdateRoomRequestValidator : Validator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid room ID.");
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Please provide a room number.")
            .MaximumLength(50).WithMessage("Room number must be 50 characters or fewer.");
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");
        RuleFor(x => x.RoomTypeId)
            .NotNull().WithMessage("Please provide a valid room type ID.");
        RuleFor(x => x.BuildingId)
            .NotNull().WithMessage("Please provide a valid building ID.");
    }
}

[HttpPut("{id:int}")]
[Group<RoomsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateRoomPermission)]
public class UpdateEndpoint : Endpoint<UpdateRoomRequest, OkOrNotFoundApiResult<UpdateRoomResponse>>
{
    private readonly IMediator _mediator;

    public UpdateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<UpdateRoomResponse>>
        ExecuteAsync(UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateRoom.Command(
                RoomId.From(request.Id),
                request.RoomNumber,
                request.Capacity,
                RoomTypeId.From(request.RoomTypeId),
                BuildingId.From(request.BuildingId)),
            cancellationToken);

        return result.ToUpdateResult(
            id => new UpdateRoomResponse
            {
                Id = id.Value,
                RoomNumber = request.RoomNumber,
                Capacity = request.Capacity,
                RoomTypeId = request.RoomTypeId,
                BuildingId = request.BuildingId,
            });
    }
}
