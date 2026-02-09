using Enrollify.Application.SubjectEquivalences.Features;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class AddSubjectsToEquivalenceGroupRequest
{
    [Microsoft.AspNetCore.Mvc.FromRoute]
    public int Id { get; set; }
    public List<string> SubjectCodes { get; set; } = new List<string>();
}

public class AddSubjectsToEquivalenceGroupRequestValidator : Validator<AddSubjectsToEquivalenceGroupRequest>
{
    public AddSubjectsToEquivalenceGroupRequestValidator()
    {
        RuleFor(x => x.SubjectCodes)
            .NotEmpty().WithMessage("Please provide at least one subject code.")
            .Must(codes => codes.All(code => !string.IsNullOrWhiteSpace(code)))
            .WithMessage("Subject codes cannot be empty or whitespace.");
    }
}

[HttpPut("{Id}/add-subjects")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectEquivalenceGroupPermission)]
public class AddSubjectsToEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<AddSubjectsToEquivalenceGroupRequest, Results<Ok<int>, NotFound, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<Ok<int>, NotFound, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(AddSubjectsToEquivalenceGroupRequest request, CancellationToken ct)
    {
        var subjectCodes = request.SubjectCodes.Select(code => SubjectCode.From(code)).ToList();
        var result = await mediator.Send(new AddSubjectsToEquivalenceGroup
            .Command(SubjectEquivalenceGroupId.From(request.Id), subjectCodes), ct);
        return result.ToUpdateResult(id => request.Id);
    }
}