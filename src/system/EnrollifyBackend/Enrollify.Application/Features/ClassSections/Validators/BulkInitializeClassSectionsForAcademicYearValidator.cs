using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using FluentValidation;

namespace Enrollify.Application.Features.ClassSections.Validators;

public class BulkInitializeClassSectionsForAcademicYearValidator : AbstractValidator<BulkInitializeClassSectionsForAcademicYear.Command>
{
    private readonly IReadRepository<Course> _courseRepository;
    private readonly IReadRepository<AcademicYear> _academicYearRepository;
    private readonly IReadRepository<Curriculum> _curriculumRepository;

    public BulkInitializeClassSectionsForAcademicYearValidator(
        IReadRepository<Course> courseRepository,
        IReadRepository<AcademicYear> academicYearRepository,
        IReadRepository<Curriculum> curriculumRepository)
    {
        _courseRepository = courseRepository;
        _academicYearRepository = academicYearRepository;
        _curriculumRepository = curriculumRepository;

        RuleFor(x => x.requestPayload)
            .NotEmpty()
            .WithMessage("At least one payload entry is required.");

        RuleForEach(x => x.requestPayload)
            .ChildRules(payload =>
            {
                payload.RuleFor(x => x.numberOfSections)
                    .GreaterThan(0)
                    .WithMessage("Number of sections must be greater than zero.");
            });

        RuleFor(x => x.requestPayload)
            .MustAsync(ValidateEntitiesExistAsync)
            .WithMessage("One or more entries reference entities that do not exist.")
            .When(x => x.requestPayload is { Count: > 0 });
    }

    private async Task<bool> ValidateEntitiesExistAsync(
        BulkInitializeClassSectionsForAcademicYear.Command command,
        List<BulkInitializeClassSectionsForAcademicYear.Payload> payloads,
        ValidationContext<BulkInitializeClassSectionsForAcademicYear.Command> context,
        CancellationToken cancellationToken)
    {
        var courseIds = payloads
            .Select(p => p.courseId)
            .Where(id => id != CourseId.From(0))
            .Distinct()
            .ToList();
        var academicYearIds = payloads
            .Select(p => p.academicYearId)
            .Where(id => id != AcademicYearId.From(0))
            .Distinct()
            .ToList();
        var curriculumIds = payloads
            .Select(p => p.curriculumId)
            .Where(id => id != CurriculumId.From(0))
            .Distinct()
            .ToList();

        var existingCoursesTask = courseIds.Count > 0
            ? _courseRepository.ListAsync(new BulkGetMinimumCoursesByIdsSpec(courseIds), cancellationToken)
            : Task.FromResult<List<Course>>([]);
        var existingAcademicYearsTask = academicYearIds.Count > 0 
            ? _academicYearRepository.ListAsync(new BulkGetAcademicYearsByIdsSpec(academicYearIds), cancellationToken)
            : Task.FromResult<List<AcademicYear>> ([]);
        var existingCurriculaTask = curriculumIds.Count > 0
            ? _curriculumRepository.ListAsync(new BulkGetMinimumCurriculumsByIdsSpec(curriculumIds), cancellationToken)
            : Task.FromResult<List<Curriculum>>([]);

        await Task.WhenAll(existingCoursesTask, existingAcademicYearsTask, existingCurriculaTask);

        var existingCourseIds = existingCoursesTask.Result.Select(c => c.Id).ToHashSet();
        var existingAcademicYearIds = existingAcademicYearsTask.Result.Select(ay => ay.Id).ToHashSet();
        var existingCurriculaById = existingCurriculaTask.Result.ToDictionary(c => c.Id);

        var isValid = true;

        for (var i = 0; i < payloads.Count; i++)
        {
            var payload = payloads[i];

            if (!existingCourseIds.Contains(payload.courseId))
            {
                context.AddFailure($"requestPayload[{i}].courseId", $"Course with ID '{payload.courseId}' does not exist.");
                isValid = false;
            }

            if (!existingAcademicYearIds.Contains(payload.academicYearId))
            {
                context.AddFailure($"requestPayload[{i}].academicYearId", $"Academic year with ID '{payload.academicYearId}' does not exist.");
                isValid = false;
            }

            if (payload.curriculumId == CurriculumId.From(0))
                continue;

            if (!existingCurriculaById.TryGetValue(payload.curriculumId, out var curriculum))
            {
                context.AddFailure($"requestPayload[{i}].curriculumId", $"Curriculum with ID '{payload.curriculumId}' does not exist.");
                isValid = false;
            }
            else if (curriculum.CourseId != payload.courseId)
            {
                context.AddFailure($"requestPayload[{i}].curriculumId", $"Curriculum with ID '{payload.curriculumId}' does not belong to course with ID '{payload.courseId}'.");
                isValid = false;
            }
        }

        return isValid;
    }
}
