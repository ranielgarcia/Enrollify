using Enrollify.Application;
using Enrollify.Application.Curriculums.DTOs;
using Enrollify.Application.Curriculums.Features;

namespace Enrollify.WebAPI.Features.Curriculums;


//public class ListCurriculumnResponse : CurriculumDTO
//{
//    public string ObfuscatedId { get; set; }
//}

//public class ListCurriculumnResponseMapper : Mapper<>

[HttpGet("")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCoursesPermission)]
public class ListCurriculumEndpoint (IMediator mediator) : EndpointWithoutRequest<List<CurriculumDTO>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListCurriculumsQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
