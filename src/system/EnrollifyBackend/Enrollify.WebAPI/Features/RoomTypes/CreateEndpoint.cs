using Enrollify.Application.RoomTypes.Features;

namespace Enrollify.WebAPI.Features.RoomTypes;

public class CreateRoomTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateRoomTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateRoomTypeRequestValidator : Validator<CreateRoomTypeRequest>
{
    public CreateRoomTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a room type name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please provide a room type description.")
            .MaximumLength(255).WithMessage("Description must be 255 characters or fewer.");
    }
}

[HttpPost("")]
[Group<RoomTypeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasCreateRoomTypePermission)]
public class CreateEndpoint : Endpoint<CreateRoomTypeRequest, CreatedApiResult<CreateRoomTypeResponse>>
{
    private readonly IMediator _mediator;

    public CreateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<CreateRoomTypeResponse>> 
        ExecuteAsync (CreateRoomTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRoomType.Command(request.Name, request.Description), cancellationToken);

        return result.ToCreatedResult(
            id => $"/room-types/{id}",
            id => new CreateRoomTypeResponse
            {
                Id = id.Value,
                Name = request.Name,
                Description = request.Description
            });
    }
}
