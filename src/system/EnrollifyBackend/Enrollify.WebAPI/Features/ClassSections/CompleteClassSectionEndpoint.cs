using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSections;

public class CompleteClassSectionRequest
{
    public int Id { get; set; }
}

public class CompleteClassSectionRequestValidator : Validator<CompleteClassSectionRequest>
{
    public CompleteClassSectionRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
    }
}

[HttpPut("{id:int}/complete")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class CompleteClassSectionEndpoint(IMediator mediator)
    : Endpoint<CompleteClassSectionRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
        CompleteClassSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteClassSection.Command(ClassSectionId.From(request.Id)), cancellationToken);
        return result.ToUpdateResult(id => id.Value);
    }
}
