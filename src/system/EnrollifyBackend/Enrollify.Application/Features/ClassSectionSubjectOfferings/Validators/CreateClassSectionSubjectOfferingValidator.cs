using FluentValidation;
using static Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands.CreateClassSectionSubjectOffering;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Validators;

/// <summary>
/// Validator for the CreateClassSectionSubjectOffering command.
///
/// No schedule-conflict checks (HC-01, HC-02, HC-03) are performed here because
/// a newly created offering has no schedules yet. Conflict detection is enforced
/// by <see cref="AddMultipleSchedulesToOfferingValidator"/> when schedules are added.
/// </summary>
public class CreateClassSectionSubjectOfferingValidator : AbstractValidator<Command>
{
    public CreateClassSectionSubjectOfferingValidator()
    {
        RuleFor(x => x.DaysPerWeek)
            .GreaterThan(0)
            .WithMessage("Days per week must be greater than zero.");

        RuleFor(x => x.HoursPerDay)
            .GreaterThan(0)
            .WithMessage("Hours per day must be greater than zero.");
    }
}

