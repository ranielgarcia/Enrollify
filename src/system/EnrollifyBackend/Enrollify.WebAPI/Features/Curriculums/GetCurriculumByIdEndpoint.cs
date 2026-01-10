using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Features;

namespace Enrollify.WebAPI.Features.Curriculums;

[HttpGet("{curriculumId}")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCurriculumsPermission)]
public class GetCurriculumByIdEndpoint (IMediator mediator) : EndpointWithoutRequest<CurriculumDTO>
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        var curriculumId = Route<int>("curriculumId");
        var result = await mediator.Send(new GetCurriculumByIdQuery(curriculumId));
        await Send.OkAsync(result.Value);
    }
}
