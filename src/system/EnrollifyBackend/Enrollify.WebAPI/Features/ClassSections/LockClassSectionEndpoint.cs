using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSections;

public class LockClassSectionRequest
{
    public int Id { get; set; }
}

public class LockClassSectionRequestValidator : Validator<LockClassSectionRequest>
{
    public LockClassSectionRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
    }
}

[HttpPut("{id:int}/lock")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class LockClassSectionEndpoint(IMediator mediator)
    : Endpoint<LockClassSectionRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
        LockClassSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LockClassSectionEnrollment.Command(ClassSectionId.From(request.Id)), cancellationToken);
        return result.ToUpdateResult(id => id.Value);
    }
}
