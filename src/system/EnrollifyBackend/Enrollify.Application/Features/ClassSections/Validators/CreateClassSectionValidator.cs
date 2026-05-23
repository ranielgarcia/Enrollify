using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Extensions;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using FluentValidation;

namespace Enrollify.Application.Features.ClassSections.Validators;

public class CreateClassSectionValidator : AbstractValidator<Commands.CreateClassSection.Command>
{
    private readonly IReadRepository<Course> _courseRepository;
    private readonly IReadRepository<AcademicYear> _academicYearRepository;
    private readonly IReadRepository<Teacher> _teacherRepository;
    private readonly IReadRepository<ClassSection> _classSectionRepository;

    public CreateClassSectionValidator(
        IReadRepository<Course> courseRepository,
        IReadRepository<AcademicYear> academicYearRepository,
        IReadRepository<Teacher> teacherRepository,
        IReadRepository<ClassSection> classSectionRepository)
    {
        _courseRepository = courseRepository;
        _academicYearRepository = academicYearRepository;
        _teacherRepository = teacherRepository;
        _classSectionRepository = classSectionRepository;

        RuleFor(x => x.CourseId)
            .MustAsync(CourseExists)
            .WithMessage("The specified course does not exist.");

        RuleFor(x => x.AcademicTermId)
            .MustAsync(AcademicTermExists)
            .WithMessage("The specified academic term does not exist.");

        RuleFor(x => x.AdviserId)
            .MustAsync(AdviserExists)
            .WithMessage("The specified adviser does not exist.");

        RuleFor(x => x.StudentCapacity)
            .GreaterThan(0)
            .WithMessage("Student capacity must be greater than zero.");

        RuleFor(x => x)
            .MustAsync(ClassSectionNameIsUnique)
            .WithMessage("A class section with the same name already exists for this academic term.");
    }

    private async Task<bool> CourseExists(CourseId courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        return course != null;
    }

    private async Task<bool> AcademicTermExists(AcademicTermId academicTermId, CancellationToken cancellationToken)
    {
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(academicTermId), cancellationToken);

        if (academicYear == null) return false;

        return academicYear.AcademicTerms.Any(at => at.Id == academicTermId);
    }

    private async Task<bool> AdviserExists(TeacherId adviserId, CancellationToken cancellationToken)
    {
        var adviser = await _teacherRepository.GetByIdAsync(adviserId, cancellationToken);
        return adviser != null;
    }

    private async Task<bool> ClassSectionNameIsUnique(
        Commands.CreateClassSection.Command command,
        CancellationToken cancellationToken)
    {
        // First, get the course to construct the potential name
        var course = await _courseRepository.GetByIdAsync(command.CourseId, cancellationToken);
        if (course == null) return true; // Will be caught by CourseExists validation

        // Get existing sections to determine the next section code
        var existingSections = await _classSectionRepository.ListAsync(
            new GetExistingClassSectionsByCourseYearLevelAndTerm(
                command.YearLevel,
                command.CourseId,
                command.AcademicTermId),
            cancellationToken);

        // Calculate what the next section code would be
        var lastSectionCode = existingSections?
            .OrderByDescending(cs => cs.SectionCode)
            .FirstOrDefault()?.SectionCode;

        var nextSectionCode = lastSectionCode.GetNextSectionCode();

        // Construct the potential name
        var potentialName = $"{course.Code.Value}-{command.YearLevel}{nextSectionCode}";

        // Check if a section with this name already exists for this academic term
        var duplicate = await _classSectionRepository
            .FirstOrDefaultAsync(
                new GetClassSectionByNameAndAcademicTermSpec(potentialName, command.AcademicTermId),
                cancellationToken);

        return duplicate == null;
    }
}
