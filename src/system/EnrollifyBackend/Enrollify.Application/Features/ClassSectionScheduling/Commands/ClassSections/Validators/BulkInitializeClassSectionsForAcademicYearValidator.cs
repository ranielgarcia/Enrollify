using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using FluentValidation;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.Validators;

public class BulkInitializeClassSectionsForAcademicYearValidator : AbstractValidator<BulkInitializeClassSectionsForAcademicYear.Command>
{
    private readonly IReadRepository<Course> _courseRepository;
    private readonly IReadRepository<AcademicYear> _academicYearRepository;
    private readonly IReadRepository<CourseCurriculumAssignment> _courseCurriculumAssignmentRepository;

    public BulkInitializeClassSectionsForAcademicYearValidator(
        IReadRepository<Course> courseRepository,
        IReadRepository<AcademicYear> academicYearRepository,
        IReadRepository<CourseCurriculumAssignment> courseCurriculumAssignmentRepository)
    {
        _courseRepository = courseRepository;
        _academicYearRepository = academicYearRepository;
        _courseCurriculumAssignmentRepository = courseCurriculumAssignmentRepository;

        RuleFor(x => x.AcademicTermId)
            .NotNull()
            .NotEmpty()
            .WithMessage("Academic term is required.");

        RuleFor(x => x.YearLevel)
            .NotNull()
            .NotEmpty()
            .WithMessage("Year level is required.");

        RuleFor(x => x.TargetCourses)
            .NotEmpty()
            .WithMessage("At least one course is required.");

        // E4: Reject duplicate courseIds in a single request
        RuleFor(x => x.TargetCourses)
            .Must(targetCourse => targetCourse.Select(p => p.CourseId).Distinct().Count() == targetCourse.Count)
            .WithMessage("Duplicate course IDs are not allowed in the same bulk initialization request.")
            .When(x => x.TargetCourses is { Count: > 0 });

        RuleForEach(x => x.TargetCourses)
            .ChildRules(targetCourse =>
            {
                // E2: Explicit non-zero courseId guard replaces the silent sentinel filter
                targetCourse.RuleFor(x => x.CourseId)
                    .Must(id => id != CourseId.From(0))
                    .WithMessage("Course ID must be a valid non-zero value.");

                // E3: Upper bound added — section codes are alphabetical (A–Z)
                targetCourse.RuleFor(x => x.NumberOfSections)
                    .GreaterThan(0)
                    .WithMessage("Number of sections must be greater than zero.")
                    .LessThanOrEqualTo(26)
                    .WithMessage("Number of sections cannot exceed 26 (A–Z).");
            });

        // E5: Granular async rule with targeted message for academic term
        RuleFor(x => x.AcademicTermId)
            .MustAsync(AcademicTermExistsAsync)
            .WithMessage("The specified academic term does not exist or is inactive.");

        // E5: Granular async rule listing specific missing course IDs
        RuleFor(x => x.TargetCourses)
            .CustomAsync(ValidateCourseIdsExistAsync)
            .When(x => x.TargetCourses is { Count: > 0 });

        // E6: Validate every course has a CourseCurriculumAssignment — prevents KeyNotFoundException in handler
        RuleFor(x => x)
            .CustomAsync(ValidateCurriculumAssignmentsAsync)
            .When(x => x.TargetCourses is { Count: > 0 });
    }

    private async Task<bool> AcademicTermExistsAsync(
        AcademicTermId academicTermId,
        CancellationToken cancellationToken)
    {
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(academicTermId), cancellationToken);
        return academicYear != null;
    }

    // E5 + E7: Replaced monolithic ValidateEntitiesExistAsync; lists missing course IDs in the error message
    private async Task ValidateCourseIdsExistAsync(
        List<BulkInitializeClassSectionsForAcademicYear.TargetCourse> targetCourses,
        ValidationContext<BulkInitializeClassSectionsForAcademicYear.Command> context,
        CancellationToken cancellationToken)
    {
        var courseIds = targetCourses
            .Select(p => p.CourseId)
            .Where(id => id != CourseId.From(0))
            .Distinct()
            .ToList();

        if (courseIds.Count == 0) return;

        var existingCourses = await _courseRepository
            .ListAsync(new BulkGetMinimumCoursesByIdsSpec(courseIds), cancellationToken);

        var existingCourseIds = existingCourses.Select(c => c.Id).ToHashSet();
        var missingIds = courseIds.Where(id => !existingCourseIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            context.AddFailure(
                nameof(BulkInitializeClassSectionsForAcademicYear.Command.TargetCourses),
                $"The following course IDs do not exist: {string.Join(", ", missingIds.Select(id => id.Value))}.");
        }
    }

    // E6: Guards against the handler throwing KeyNotFoundException when a course has no curriculum assignment
    private async Task ValidateCurriculumAssignmentsAsync(
        BulkInitializeClassSectionsForAcademicYear.Command command,
        ValidationContext<BulkInitializeClassSectionsForAcademicYear.Command> context,
        CancellationToken cancellationToken)
    {
        var currentAcademicYear = await _academicYearRepository
            .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.AcademicTermId), cancellationToken);

        if (currentAcademicYear == null) return; // Already caught by AcademicTermExistsAsync

        // Derive cohort entry year — the AY when Year 1 students of this cohort started
        var cohortEntryYear = Year.From(currentAcademicYear.StartDate.Value.Year - (command.YearLevel.Value - 1));

        var cohortAcademicYear = await _academicYearRepository
            .FirstOrDefaultAsync(new GetAcademicYearByStartDateYearSpec(cohortEntryYear), cancellationToken);

        if (cohortAcademicYear == null)
        {
            context.AddFailure(
                nameof(BulkInitializeClassSectionsForAcademicYear.Command.TargetCourses),
                "No academic year exists for the cohort entry year. Ensure historical academic years are created before initializing sections for higher year levels.");
            return;
        }

        var courseIds = command.TargetCourses
            .Select(p => p.CourseId)
            .Where(id => id != CourseId.From(0))
            .Distinct()
            .ToList();

        if (courseIds.Count == 0) return;

        var assignments = await _courseCurriculumAssignmentRepository
            .ListAsync(
                new BulkGetCourseCurriculumAssignmentsByAcademicYearIdAndCourseIdsSpec(cohortAcademicYear.Id, courseIds),
                cancellationToken);

        var assignedCourseIds = assignments.Select(a => a.CourseId).ToHashSet();
        var unassignedIds = courseIds.Where(id => !assignedCourseIds.Contains(id)).ToList();

        if (unassignedIds.Count > 0)
        {
            context.AddFailure(
                nameof(BulkInitializeClassSectionsForAcademicYear.Command.TargetCourses),
                $"The following courses do not have a curriculum assignment for the cohort entry year: {string.Join(", ", unassignedIds.Select(id => id.Value))}.");
        }
    }
}
