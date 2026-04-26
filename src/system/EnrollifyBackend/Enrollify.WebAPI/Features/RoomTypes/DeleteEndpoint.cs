using Enrollify.Application.Features.RoomTypes.Commands;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.WebAPI.Features.RoomTypes;


public class DeleteRequest
{
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

[HttpDelete("{id:int}")]
[Group<RoomTypeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasDeleteRoomTypesPermission)]
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
        var result = await _mediator.Send(new DeleteRoomType.Command(RoomTypeId.From(request.Id)), ct);
        return result.ToDeleteResult();
    }
}
