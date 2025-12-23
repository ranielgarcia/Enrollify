using Enrollify.Application.Buildings.Features;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.WebAPI.Authorization;
using Enrollify.WebAPI.Extensions;
using FastEndpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features.Buildings;


public class UpdateBuildingResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class UpdateBuildingRequest
{
    [QueryParam]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
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
    }
}

[HttpPut("")]
[Group<BuildingEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateBuildingPermission)]
public class UpdateEndpoint : Endpoint<UpdateBuildingRequest, Results<Ok<UpdateBuildingResponse>, NotFound, Conflict<string[]>, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public UpdateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Ok<UpdateBuildingResponse>, NotFound, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync (UpdateBuildingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateBuilding.Command(BuildingId.From(request.Id), request.Name, request.Description, request.Address), cancellationToken);

        return result.ToUpdateResult(
             id => new UpdateBuildingResponse
             {
                 Id = id.Value,
                 Name = request.Name,
                 Description = request.Description,
                 Address = request.Address,
             });
    }
}
