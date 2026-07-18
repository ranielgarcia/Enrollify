using Enrollify.Application.Features.Buildings.Commands;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.WebAPI.Features.Buildings;


public class UpdateBuildingResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class UpdateBuildingRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class UpdateBuildingRequestValidator : Validator<UpdateBuildingRequest>
{
    public UpdateBuildingRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid building ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a building name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please provide a building description.")
            .MaximumLength(255).WithMessage("Description must be 255 characters or fewer.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Please provide a building address.")
            .MaximumLength(255).WithMessage("Address must be 255 characters or fewer.");

        RuleFor(x => x.CollegeId)
            .NotNull().WithMessage("Please provide a valid college ID.");
    }
}

[HttpPut("{id:int}")]
[Group<BuildingEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateBuildingPermission)]
public class UpdateEndpoint : Endpoint<UpdateBuildingRequest, OkOrNotFoundApiResult<UpdateBuildingResponse>>
{
    private readonly IMediator _mediator;

    public UpdateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<UpdateBuildingResponse>>
        ExecuteAsync (UpdateBuildingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateBuilding.Command(
                BuildingId.From(request.Id), 
                request.Name, 
                request.Description,
                request.Address, 
                CollegeId.From(request.CollegeId)
            ),
            cancellationToken);

        return result.ToUpdatedResult(
             id => new UpdateBuildingResponse
             {
                 Id = id.Value,
                 Name = request.Name,
                 Description = request.Description,
                 Address = request.Address,
                 CollegeId = request.CollegeId
             });
    }
}
