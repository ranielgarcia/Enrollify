using Enrollify.Application.Features.Rooms.Features;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.WebAPI.Features.Rooms;

public class CreateRoomResponse
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomTypeId { get; set; }
    public int BuildingId { get; set; }
}

public class CreateRoomRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomTypeId { get; set; }
    public int BuildingId { get; set; }
}
public class CreateRoomRequestValidator : Validator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
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

[HttpPost("")]
[Group<RoomsEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateRoomPermission)]
public class CreateEndpoint : Endpoint<CreateRoomRequest, CreatedApiResult<CreateRoomResponse>>
{
    private readonly IMediator _mediator;

    public CreateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<CreateRoomResponse>>
        ExecuteAsync (CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateRoom.Command(request.RoomNumber, request.Capacity, RoomTypeId.From(request.RoomTypeId), BuildingId.From(request.BuildingId)), 
                cancellationToken);

        return result.ToCreatedResult(
            id => $"/rooms/{id}",
            id => new CreateRoomResponse
            {
                Id = id.Value,
                RoomNumber = request.RoomNumber,
                Capacity = request.Capacity,
                RoomTypeId = request.RoomTypeId,
                BuildingId = request.BuildingId,
            });
    }
}
