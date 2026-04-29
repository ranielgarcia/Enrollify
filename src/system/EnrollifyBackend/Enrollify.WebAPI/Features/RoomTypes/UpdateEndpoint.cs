using Enrollify.Application.Features.RoomTypes.Commands;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.WebAPI.Features.RoomTypes;

public class UpdateRoomTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoomTypeRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoomTypeRequestValidator : Validator<UpdateRoomTypeRequest>
{
    public UpdateRoomTypeRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid room type ID.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a room type name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be 500 characters or fewer.");
    }
}

[HttpPut("{id:int}")]
[Group<RoomTypeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasUpdateRoomTypesPermission)]
public class UpdateEndpoint : Endpoint<UpdateRoomTypeRequest, OkOrNotFoundApiResult<UpdateRoomTypeResponse>>
{
    private readonly IMediator _mediator;

    public UpdateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<UpdateRoomTypeResponse>> 
        ExecuteAsync (UpdateRoomTypeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateRoomType.Command(RoomTypeId.From(request.Id), request.Name, request.Description));

        return result.ToUpdateResult(
            id => new UpdateRoomTypeResponse
            {
                Id = id.Value,
                Name = request.Name,
                Description = request.Description
            });
    }
}
