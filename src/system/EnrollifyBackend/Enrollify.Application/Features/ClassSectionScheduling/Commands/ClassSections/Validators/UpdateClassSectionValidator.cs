using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using FluentValidation;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.Validators;

public class UpdateClassSectionValidator : AbstractValidator<UpdateClassSection.Command>
{
    private readonly IReadRepository<Teacher> _teacherRepository;

    public UpdateClassSectionValidator(IReadRepository<Teacher> teacherRepository)
    {
        _teacherRepository = teacherRepository;

        RuleFor(x => x.AdviserId)
            .MustAsync(AdviserExists)
            .WithMessage("The specified adviser does not exist.");
    }

    private async Task<bool> AdviserExists(TeacherId adviserId, CancellationToken cancellationToken)
    {
        var adviser = await _teacherRepository.GetByIdAsync(adviserId, cancellationToken);
        return adviser is not null;
    }
}
