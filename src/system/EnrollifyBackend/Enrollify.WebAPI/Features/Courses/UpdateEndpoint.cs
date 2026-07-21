using Enrollify.Application.Features.Courses.Commands;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.WebAPI.Features.Courses;


public class UpdateCourseResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DurationYears { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class UpdateCourseRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DurationYears { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class UpdateCourseRequestValidator : Validator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid course ID.");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Please provide a course code.")
            .MaximumLength(10).WithMessage("Code must be 10 characters or fewer.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a course name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");
        RuleFor(x => x.Description)
            .MaximumLength(1500).WithMessage("Description must be 1500 characters or fewer.");
        RuleFor(x => x.DurationYears)
            .InclusiveBetween(1, 10)
            .WithMessage("Duration must be between 1 and 10 years.");
        RuleFor(x => x.CollegeId)
            .NotNull().WithMessage("Please provide a valid college ID.");
    }
}

[HttpPut("{id:int}")]
[Group<CourseEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateCoursePermission)]
public class UpdateEndpoint (IMediator mediator)
    : Endpoint<UpdateCourseRequest, OkOrNotFoundApiResult<UpdateCourseResponse>>
{
    public override async Task<OkOrNotFoundApiResult<UpdateCourseResponse>>
        ExecuteAsync(UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCourse.Command(
            CourseId.From(request.Id),
            CourseCode.From(request.Code),
            request.Name,
            request.DurationYears,
            request.Description,
            CollegeId.From(request.CollegeId)
        ), cancellationToken);

        return result.ToUpdatedResult(
            id => new UpdateCourseResponse
            {
                Id = id.Value,
                Code = request.Code,
                Name = request.Name,
                DurationYears = request.DurationYears,
                Description = request.Description,
                CollegeId = request.CollegeId
            });
    }
}
