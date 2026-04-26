using Enrollify.Application.Features.Courses.Queries;
using Enrollify.Application.Features.Courses.DTOs;

namespace Enrollify.WebAPI.Features.Courses;

[HttpGet("")]
[Group<CourseEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewCoursesPermission)]
public class ListCoursesEndpoint(IMediator mediator) : EndpointWithoutRequest<List<CourseDto>>
{
    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await mediator.Send(new ListCoursesQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
