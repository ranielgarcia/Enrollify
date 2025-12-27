using Enrollify.Application.Courses.DTOs;
using Enrollify.Application.Courses.Features;

namespace Enrollify.WebAPI.Features.Courses;

[HttpGet("")]
[Group<CourseEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCoursesPermission)]
public class ListCoursesEndpoint(IMediator mediator) : EndpointWithoutRequest<List<CourseDTO>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListCoursesQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
