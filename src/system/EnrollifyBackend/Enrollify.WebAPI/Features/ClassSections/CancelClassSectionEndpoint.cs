using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSections;

public class CancelClassSectionRequest
{
    public int Id { get; set; }
}

public class CancelClassSectionRequestValidator : Validator<CancelClassSectionRequest>
{
    public CancelClassSectionRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
    }
}

[HttpPut("{id:int}/cancel")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class CancelClassSectionEndpoint(IMediator mediator)
    : Endpoint<CancelClassSectionRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
        CancelClassSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CancelClassSection.Command(ClassSectionId.From(request.Id)), cancellationToken);
        return result.ToUpdateResult(id => id.Value);
    }
}
