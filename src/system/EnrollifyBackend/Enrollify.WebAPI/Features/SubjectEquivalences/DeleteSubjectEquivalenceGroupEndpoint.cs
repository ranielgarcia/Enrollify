using Enrollify.Application.SubjectEquivalences.Features;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class DeleteSubjectEquivalenceGroupRequest
{
    [Microsoft.AspNetCore.Mvc.FromRoute]
    public int Id { get; set; }
}

[HttpDelete("{id}")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteSubjectEquivalenceGroupPermission)]
public class DeleteSubjectEquivalenceGroupEndpoint (IMediator mediator)
    : Endpoint<DeleteSubjectEquivalenceGroupRequest, Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(DeleteSubjectEquivalenceGroupRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteSubjectEquivalenceGroup.Command(SubjectEquivalenceGroupId.From(request.Id)), ct);
        return result.ToDeleteResult();
    }
}
