using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.WebAPI.Features.Curriculums;

public class CrateDraftCurriculumRequest
{
    public CourseId CourseId { get; set; }
    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;
    public string? Description { get; set; }
}


public class CrateDraftCurriculumRequestValidator : Validator<CrateDraftCurriculumRequest>
{
    public CrateDraftCurriculumRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotNull().WithMessage("Please provide a valid course ID.");

        RuleFor(x => x.EffectiveYear)
            .NotNull().WithMessage("Please provide effective year.")
            .GreaterThanOrEqualTo(2000).WithMessage("Please enter an effective year greater than or equal to 2000.");

        RuleFor(x => x.Version)
        .NotEmpty().WithMessage("Please provide a curriculum description.")
        .MaximumLength(20).WithMessage("Description must be 20 characters or fewer.");

        RuleFor(x => x.Description)
        .NotEmpty().WithMessage("Please provide a curriculum description.")
        .MaximumLength(500).WithMessage("Description must be 500 characters or fewer.");
    }
}

public class CreateDraftCurriculumEndpoint
{
}
