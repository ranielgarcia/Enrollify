using Enrollify.Application.Courses.Features;
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
public class DeleteEndpoint(IMediator mediator) : Endpoint<DeleteCourseRequest, Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(DeleteCourseRequest request, CancellationToken ct)
    { 
        var result = await mediator.Send(new DeleteCourse.Command(CourseId.From(request.Id)));
        return result.ToDeleteResult();
    }
}
