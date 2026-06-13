using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSections;

public class ActivateClassSectionRequest
{
    public int Id { get; set; }
}

public class ActivateClassSectionRequestValidator : Validator<ActivateClassSectionRequest>
{
    public ActivateClassSectionRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
    }
}

[HttpPut("{id:int}/activate")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class ActivateClassSectionEndpoint(IMediator mediator)
    : Endpoint<ActivateClassSectionRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
        ActivateClassSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ActivateClassSection.Command(ClassSectionId.From(request.Id)), cancellationToken);
        return result.ToUpdateResult(id => id.Value);
    }
}
