using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Features;

namespace Enrollify.WebAPI.Features.Subjects;

[HttpGet("")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class ListMinimalSubjectsEndpoint(IMediator mediator) : EndpointWithoutRequest<List<SubjectDTO>>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectsMinimalQuery(), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
