using Enrollify.Application.Features.SubjectEquivalences.DTOs;
using Enrollify.Application.Features.SubjectEquivalences.Queries;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

[HttpGet("")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectEquivalenceGroupsPermission)]
public class ListSubjectEquivalenceGroupsEndpoint (IMediator mediator) : EndpointWithoutRequest<List<SubjectEquivalenceGroupDto>>
{
    public override async Task HandleAsync (CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectEquivalenceGroupsQuery(), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
