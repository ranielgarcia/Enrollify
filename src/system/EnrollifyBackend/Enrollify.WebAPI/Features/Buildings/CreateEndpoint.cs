using Enrollify.Application.Buildings.Features;
using Enrollify.WebAPI.Authorization;
using Enrollify.WebAPI.Extensions;
using FastEndpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features.Buildings;

public class CreateBuildingResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class CreateBuildingRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class CreateBuildingRequestValidator : Validator<CreateBuildingRequest>
{
    public CreateBuildingRequestValidator()
    {
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


[HttpPost("")]
[Group<BuildingEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateBuildingPermission)]
public class CreateEndpoint : Endpoint<CreateBuildingRequest, Results<Created<CreateBuildingResponse>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public CreateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Created<CreateBuildingResponse>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync (CreateBuildingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateBuilding.Command(request.Name, request.Description, request.Address), cancellationToken);

        return result.ToCreatedResult(
            id => $"/buildings/{id}",
            id => new CreateBuildingResponse
            {
                Id = id.Value,
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
            });
    }
}
