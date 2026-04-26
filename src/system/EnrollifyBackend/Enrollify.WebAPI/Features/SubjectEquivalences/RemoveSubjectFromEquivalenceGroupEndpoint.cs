using Enrollify.Application.Features.SubjectEquivalences.Commands;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class RemoveSubjectFromEquivalenceGroupRequest
{
    public int Id { get; set; }
    public string SubjectCode { get; set; } = null!;
}

public class RemoveSubjectFromEquivalenceGroupRequestValidator : Validator<RemoveSubjectFromEquivalenceGroupRequest>
{
    public RemoveSubjectFromEquivalenceGroupRequestValidator()
    {
        RuleFor(x => x.SubjectCode)
            .NotEmpty().WithMessage("Please provide a subject code.")
            .MaximumLength(50).WithMessage("Subject code must be 50 characters or fewer.");
    }
}

[HttpDelete("{Id}/remove-subject")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectEquivalenceGroupPermission)]
public class RemoveSubjectFromEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<RemoveSubjectFromEquivalenceGroupRequest, DeleteApiResult>
{
    public override async Task<DeleteApiResult>
        ExecuteAsync(RemoveSubjectFromEquivalenceGroupRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveSubjectFromEquivalenceGroup
            .Command(SubjectEquivalenceGroupId.From(request.Id), SubjectCode.From(request.SubjectCode)), ct);
        return result.ToDeleteResult();
    }
}
