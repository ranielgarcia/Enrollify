using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.WebAPI.Features.ClassSections;

public class CreateClassSectionRequest
{
    public int YearLevel { get; set; }
    public int CourseId { get; set; }
    public int AcademicTermId { get; set; }
    public int AdviserId { get; set; }
    public int StudentCapacity { get; set; }
}

public class CreateClassSectionRequestValidator : Validator<CreateClassSectionRequest>
{
    public CreateClassSectionRequestValidator()
    {
        RuleFor(x => x.YearLevel)
            .GreaterThan(0).WithMessage("Year level must be greater than zero.")
            .LessThanOrEqualTo(6).WithMessage("Year level must be 6 or less.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.");

        RuleFor(x => x.AcademicTermId)
            .NotEmpty().WithMessage("Academic term ID is required.");

        RuleFor(x => x.AdviserId)
            .NotEmpty().WithMessage("Adviser ID is required.");

        RuleFor(x => x.StudentCapacity)
            .GreaterThan(0).WithMessage("Student capacity must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Student capacity cannot exceed 100.");
    }
}

[HttpPost("")]
[Group<ClassSectionsEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateClassSectionPermission)]
public class CreateClassSectionEndpoint : Endpoint<CreateClassSectionRequest, CreatedApiResult<int>>
{
    private readonly IMediator _mediator;

    public CreateClassSectionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<int>>
        ExecuteAsync (CreateClassSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateClassSection.Command(
                YearLevel.From(request.YearLevel),
                CourseId.From(request.CourseId),
                AcademicTermId.From(request.AcademicTermId),
                TeacherId.From(request.AdviserId),
                request.StudentCapacity), cancellationToken);

        return result.ToCreatedResult(
            id => $"/class-sections/{id.Value}",
            id => id.Value);
    }
}
