using Enrollify.Application.Features.Curriculums.Queries;
using Enrollify.Application.Features.Curriculums.DTOs;

namespace Enrollify.WebAPI.Features.Curriculums;

[HttpGet("")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCurriculumsPermission)]
public class ListCurriculumEndpoint(IMediator mediator) : EndpointWithoutRequest<List<CurriculumDto>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListCurriculumsQuery(), c);

        await Send.OkAsync(result.Value);
    }
}
