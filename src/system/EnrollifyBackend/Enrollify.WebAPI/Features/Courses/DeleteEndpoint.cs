using Enrollify.Application.Features.Courses.Commands;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.WebAPI.Features.Courses;

public class DeleteCourseRequest
{
    [QueryParam]
    public int Id { get; set; }
}


public class DeleteCourseRequestValidator : Validator<DeleteCourseRequest>
{
    public DeleteCourseRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid course ID.");
    }
}

[HttpDelete("")]
[Group<CourseEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteCoursePermission)]
public class DeleteEndpoint(IMediator mediator) : Endpoint<DeleteCourseRequest, DeleteApiResult>
{
    public override async Task<DeleteApiResult>
        ExecuteAsync(DeleteCourseRequest request, CancellationToken ct)
    { 
        var result = await mediator.Send(new DeleteCourse.Command(CourseId.From(request.Id)));
        return result.ToDeleteResult();
    }
}
