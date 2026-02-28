using Enrollify.Application.Courses.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.WebAPI.Features.Courses;

public class CreateCourseResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DurationYears { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class CreateCourseRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DurationYears { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class CreateCourseRequestValidator : Validator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
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

[HttpPost("")]
[Group<CourseEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateCoursePermission)]
public class CreateEndpoint(IMediator mediator)
    : Endpoint<CreateCourseRequest, CreatedApiResult<CreateCourseResponse>>
{
    public override async Task<CreatedApiResult<CreateCourseResponse>>
        ExecuteAsync(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCourse.Command(
            CourseCode.From(request.Code),
            request.Name,
            request.DurationYears,
            request.Description,
            CollegeId.From(request.CollegeId)
        ), cancellationToken);

        return result.ToCreatedResult(
            id => $"/courses/{id}",
            id => new CreateCourseResponse
            {
                Id = id.Value,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                DurationYears = request.DurationYears,
                CollegeId = request.CollegeId
            });
    }
}
