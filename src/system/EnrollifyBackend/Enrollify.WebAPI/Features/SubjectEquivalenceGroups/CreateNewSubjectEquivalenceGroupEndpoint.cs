using Enrollify.Application.SubjectEquivalenceGroups.Features;

namespace Enrollify.WebAPI.Features.SubjectEquivalenceGroups;

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
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");
    }
}

[HttpPost("")]
[Group<CreateNewSubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateSubjectEquivalenceGroupPermission)]
public class CreateNewSubjectEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<CreateNewSubjectEquivalenceGroupRequest, Results<Created<int>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<Created<int>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(CreateNewSubjectEquivalenceGroupRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddNewSubjectEquivalenceGroup
            .Command(request.Name), ct);
        return result.ToCreatedResult(
            id => $"/subject-equivalence-groups/{id}",
            id => id.Value);
    }
}
