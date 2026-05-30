using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSections;

public class DeleteClassSectionRequest
{
    public int Id { get; set; }
    public bool ReOrderClassSectionCodes { get; set; }
}

[HttpDelete("{id:int}")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteClassSectionPermission)]
public class DeleteClassSectionEndpoint : Endpoint<DeleteClassSectionRequest, DeleteApiResult>
{
    private readonly IMediator _mediator;

    public DeleteClassSectionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<DeleteApiResult> ExecuteAsync(DeleteClassSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteClassSection.Command(
                ClassSectionId.From(request.Id),
                request.ReOrderClassSectionCodes),
            cancellationToken);

        return result.ToDeleteResult();
    }
}
