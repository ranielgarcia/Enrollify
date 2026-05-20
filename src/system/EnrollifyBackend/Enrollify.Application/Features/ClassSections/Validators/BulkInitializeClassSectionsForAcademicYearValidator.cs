using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.SharedKernel;
using FluentValidation;

namespace Enrollify.Application.Features.ClassSections.Validators;

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

        RuleFor(x => x.academicTermId)
            .NotNull()
            .NotEmpty()
            .WithMessage("Academic term is required.");

        RuleFor(x => x.yearLevel)
            .NotNull()
            .NotEmpty()
            .WithMessage("Year level is required.");

        RuleFor(x => x.requestPayload)
            .NotEmpty()
            .WithMessage("At least one payload entry is required.");

        // E4: Reject duplicate courseIds in a single request
        RuleFor(x => x.requestPayload)
            .Must(payload => payload.Select(p => p.courseId).Distinct().Count() == payload.Count)
            .WithMessage("Duplicate course IDs are not allowed in the same bulk initialization request.")
            .When(x => x.requestPayload is { Count: > 0 });

        RuleForEach(x => x.requestPayload)
            .ChildRules(payload =>
            {
                // E2: Explicit non-zero courseId guard replaces the silent sentinel filter
                payload.RuleFor(x => x.courseId)
                    .Must(id => id != CourseId.From(0))
                    .WithMessage("Course ID must be a valid non-zero value.");

                // E3: Upper bound added — section codes are alphabetical (A–Z)
                payload.RuleFor(x => x.numberOfSections)
                    .GreaterThan(0)
                    .WithMessage("Number of sections must be greater than zero.")
                    .LessThanOrEqualTo(26)
                    .WithMessage("Number of sections cannot exceed 26 (A–Z).");
            });

        // E5: Granular async rule with targeted message for academic term
        RuleFor(x => x.academicTermId)
            .MustAsync(AcademicTermExistsAsync)
            .WithMessage("The specified academic term does not exist or is inactive.");

        // E5: Granular async rule listing specific missing course IDs
        RuleFor(x => x.requestPayload)
            .CustomAsync(ValidateCourseIdsExistAsync)
            .When(x => x.requestPayload is { Count: > 0 });

        // E6: Validate every course has a CourseCurriculumAssignment — prevents KeyNotFoundException in handler
        RuleFor(x => x)
            .CustomAsync(ValidateCurriculumAssignmentsAsync)
            .When(x => x.requestPayload is { Count: > 0 });
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
        List<BulkInitializeClassSectionsForAcademicYear.Payload> payloads,
        ValidationContext<BulkInitializeClassSectionsForAcademicYear.Command> context,
        CancellationToken cancellationToken)
    {
        var courseIds = payloads
            .Select(p => p.courseId)
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
                nameof(BulkInitializeClassSectionsForAcademicYear.Command.requestPayload),
                $"The following course IDs do not exist: {string.Join(", ", missingIds.Select(id => id.Value))}.");
        }
    }

    // E6: Guards against the handler throwing KeyNotFoundException when a course has no curriculum assignment
    private async Task ValidateCurriculumAssignmentsAsync(
        BulkInitializeClassSectionsForAcademicYear.Command command,
        ValidationContext<BulkInitializeClassSectionsForAcademicYear.Command> context,
        CancellationToken cancellationToken)
    {
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.academicTermId), cancellationToken);

        if (academicYear == null) return; // Already caught by AcademicTermExistsAsync

        var courseIds = command.requestPayload
            .Select(p => p.courseId)
            .Where(id => id != CourseId.From(0))
            .Distinct()
            .ToList();

        if (courseIds.Count == 0) return;

        var assignments = await _courseCurriculumAssignmentRepository
            .ListAsync(
                new BulkGetCourseCurriculumAssignmentsByAcademicYearIdAndCourseIdsSpec(academicYear.Id, courseIds),
                cancellationToken);

        var assignedCourseIds = assignments.Select(a => a.CourseId).ToHashSet();
        var unassignedIds = courseIds.Where(id => !assignedCourseIds.Contains(id)).ToList();

        if (unassignedIds.Count > 0)
        {
            context.AddFailure(
                nameof(BulkInitializeClassSectionsForAcademicYear.Command.requestPayload),
                $"The following courses do not have a curriculum assignment for the specified academic year: {string.Join(", ", unassignedIds.Select(id => id.Value))}.");
        }
    }
}
