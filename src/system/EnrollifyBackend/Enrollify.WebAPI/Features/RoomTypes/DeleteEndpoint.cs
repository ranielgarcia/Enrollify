using Enrollify.Application.RoomTypes.Features;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.WebAPI.Features.RoomTypes;


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
            .GreaterThan(0).WithMessage("Please provide a valid room type ID.");
    }
}

[HttpDelete("")]
[Group<RoomTypeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasDeleteRoomTypesPermission)]
public class DeleteEndpoint : Endpoint<DeleteRequest, Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public DeleteEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }


    public override async Task<Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>> 
        ExecuteAsync (DeleteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRoomType.Command(RoomTypeId.From(request.Id)), cancellationToken);
        return result.ToDeleteResult();
    }
}
