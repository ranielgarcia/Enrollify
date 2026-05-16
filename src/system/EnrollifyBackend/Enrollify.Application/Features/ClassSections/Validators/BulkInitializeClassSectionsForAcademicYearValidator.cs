using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;
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

        RuleForEach(x => x.requestPayload)
            .ChildRules(payload =>
            {
                payload.RuleFor(x => x.numberOfSections)
                    .GreaterThan(0)
                    .WithMessage("Number of sections must be greater than zero.");
            });

        RuleFor(x => x)
            .MustAsync((command, ct) => ValidateEntitiesExistAsync(command, ct))
            .WithMessage("One or more entries reference entities that do not exist.");
    }

    private async Task<bool> ValidateEntitiesExistAsync(
        BulkInitializeClassSectionsForAcademicYear.Command command,
        CancellationToken cancellationToken)
    {
        var payloads = command.requestPayload;
        var courseIds = payloads
            .Select(p => p.courseId)
            .Where(id => id != CourseId.From(0))
            .Distinct()
            .ToList();
        var curriculumIds = payloads
            .Select(p => p.curriculumId)
            .Where(id => id != CurriculumId.From(0))
            .Distinct()
            .ToList();

        // Validate academic term exists
        var academicYearTask = _academicYearRepository
            .FirstOrDefaultAsync(new GetAcademicYearByAcademicTermIdSpec(command.academicTermId), cancellationToken);
        var existingCoursesTask = courseIds.Count > 0
            ? _courseRepository.ListAsync(new BulkGetMinimumCoursesByIdsSpec(courseIds), cancellationToken)
            : Task.FromResult<List<Course>>([]);
        var existingCurriculaTask = curriculumIds.Count > 0
            ? _curriculumRepository.ListAsync(new BulkGetMinimumCurriculumsByIdsSpec(curriculumIds), cancellationToken)
            : Task.FromResult<List<Curriculum>>([]);

        await Task.WhenAll(academicYearTask, existingCoursesTask, existingCurriculaTask);

        var isValid = true;

        // Validate academic term
        if (academicYearTask.Result == null)
        {
            return false;
        }

        var existingCourseIds = existingCoursesTask.Result.Select(c => c.Id).ToHashSet();
        var existingCurriculaById = existingCurriculaTask.Result.ToDictionary(c => c.Id);

        for (var i = 0; i < payloads.Count; i++)
        {
            var payload = payloads[i];

            if (!existingCourseIds.Contains(payload.courseId))
            {
                isValid = false;
            }

            if (payload.curriculumId == CurriculumId.From(0))
                continue;

            if (!existingCurriculaById.TryGetValue(payload.curriculumId, out var curriculum))
            {
                isValid = false;
            }
            else if (curriculum.CourseId != payload.courseId)
            {
                isValid = false;
            }
        }

        return isValid;
    }
}
