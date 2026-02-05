using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Features;

namespace Enrollify.WebAPI.Features.Curriculums;

[HttpGet("")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCurriculumsPermission)]
public class ListCurriculumEndpoint(IMediator mediator) : EndpointWithoutRequest<List<CurriculumDTO>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListCurriculumsQuery(), c);

        await Send.OkAsync(result.Value);
    }
}
