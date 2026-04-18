using Enrollify.Application.Features.SubjectEquivalences.Features;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class CreateNewSubjectEquivalenceGroupRequest
{
    public required string Name { get; set; }
}

public class CreateNewSubjectEquivalenceGroupRequestValidator : Validator<CreateNewSubjectEquivalenceGroupRequest>
{
    public CreateNewSubjectEquivalenceGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a name for the subject equivalence group.")
            .MaximumLength(255).WithMessage("Name must be 255 characters or fewer.");
    }
}

[HttpPost("")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateSubjectEquivalenceGroupPermission)]
public class CreateNewSubjectEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<CreateNewSubjectEquivalenceGroupRequest, CreatedApiResult<int>>
{
    public override async Task<CreatedApiResult<int>>
        ExecuteAsync(CreateNewSubjectEquivalenceGroupRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddNewSubjectEquivalenceGroup
            .Command(request.Name), ct);
        return result.ToCreatedResult(
            id => $"/subject-equivalence-groups/{id}",
            id => id.Value);
    }
}
