using Enrollify.Application.Colleges.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.WebAPI.Features.Colleges;

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
            .GreaterThan(0).WithMessage("Please provide a valid college ID.");
    }
}

[HttpDelete("")]
[Group<CollegeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasDeleteCollegePermission)]
public class DeleteEndpoint : Endpoint<DeleteRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public DeleteEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult> 
        ExecuteAsync (DeleteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCollege.Command(CollegeId.From(request.Id)));
        return result.ToDeleteResult();
    }
}
