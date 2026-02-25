using Enrollify.Application.SubjectEquivalences.DTOs;
using Enrollify.Application.SubjectEquivalences.Features;

namespace Enrollify.WebAPI.Features.SubjectEquivalences;

[HttpGet("")]
[Group<SubjectEquivalenceGroupEndpointGroup>]
//[Authorize(Policy = PolicyName.HasViewSubjectEquivalenceGroupsPermission)]
[AllowAnonymous]
public class ListSubjectEquivalenceGroupsEndpoint (IMediator mediator) : EndpointWithoutRequest<List<SubjectEquivalenceGroupDTO>>
{
    public override async Task HandleAsync (CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectEquivalenceGroupsQuery(), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
