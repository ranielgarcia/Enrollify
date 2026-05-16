using Enrollify.Application.Features.Curriculums.DTOs;
using Enrollify.Application.Features.Curriculums.Queries;

namespace Enrollify.WebAPI.Features.Curriculums;

[HttpGet("latest-active")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCurriculumsPermission)]
public class ListLatestActiveCurriculumsEndpoint(IMediator mediator) : EndpointWithoutRequest<List<CurriculumDto>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListLatestActiveCurriculumsQuery(), c);

        await Send.OkAsync(result.Value);
    }
}
