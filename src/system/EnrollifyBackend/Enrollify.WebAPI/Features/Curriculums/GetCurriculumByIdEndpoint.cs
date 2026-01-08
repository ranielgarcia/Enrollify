using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Features;
using Enrollify.WebAPI.Utilities;

namespace Enrollify.WebAPI.Features.Curriculums;

[HttpGet("{curriculumId}")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCurriculumsPermission)]
public class GetCurriculumByIdEndpoint (IMediator mediator, IIdObfuscator idObfuscator) : EndpointWithoutRequest<CurriculumDTO>
{
    public override async Task<CurriculumDTO> HandleAsync(CancellationToken ct)
    {
        var curriculumId = Route<string>("curriculumId");
        var result = await mediator.Send(new GetCurriculumByIdQuery(idObfuscator.Decode(curriculumId ?? "")));
        return result;
    }
}
