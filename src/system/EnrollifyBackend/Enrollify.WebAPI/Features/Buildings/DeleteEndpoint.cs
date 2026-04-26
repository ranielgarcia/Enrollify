using Enrollify.Application.Features.Buildings.Commands;
using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.WebAPI.Features.Buildings;

public class DeleteRequest
{
    [QueryParam]
    public int Id { get; set; }
}

public class DeleteRequestValidator : Validator<DeleteRequest>
{
    public DeleteRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid building ID.");
    }
}

[HttpDelete("")]
[Group<BuildingEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteBuildingPermission)]
public class DeleteEndpoint : Endpoint<DeleteRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public DeleteEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult>
        ExecuteAsync (DeleteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteBuilding.Command(BuildingId.From(request.Id)), cancellationToken);
        return result.ToDeleteResult();
    }
}
