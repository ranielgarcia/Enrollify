using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections.Commands;
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
            .SetValidator(new PayloadValidator(_courseRepository, _academicYearRepository, _curriculumRepository));
    }

    private class PayloadValidator : AbstractValidator<BulkInitializeClassSectionsForAcademicYear.Payload>
    {
        private readonly IReadRepository<Course> _courseRepository;
        private readonly IReadRepository<AcademicYear> _academicYearRepository;
        private readonly IReadRepository<Curriculum> _curriculumRepository;

        public PayloadValidator(
            IReadRepository<Course> courseRepository,
            IReadRepository<AcademicYear> academicYearRepository,
            IReadRepository<Curriculum> curriculumRepository)
        {
            _courseRepository = courseRepository;
            _academicYearRepository = academicYearRepository;
            _curriculumRepository = curriculumRepository;

            RuleFor(x => x.courseId)
                .MustAsync(CourseExists)
                .WithMessage("The specified course does not exist.");

            RuleFor(x => x.academicYearId)
                .MustAsync(AcademicYearExists)
                .WithMessage("The specified academic year does not exist.");

            RuleFor(x => x)
                .MustAsync(CurriculumBelongsToCourse)
                .WithMessage("The specified curriculum does not exist or does not belong to the specified course.")
                .When(x => x.curriculumId != CurriculumId.From(0));

            RuleFor(x => x.numberOfSections)
                .GreaterThan(0)
                .WithMessage("Number of sections must be greater than zero.");
        }

        private async Task<bool> CourseExists(CourseId courseId, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
            return course != null;
        }

        private async Task<bool> AcademicYearExists(AcademicYearId academicYearId, CancellationToken cancellationToken)
        {
            var academicYear = await _academicYearRepository
                .FirstOrDefaultAsync(new GetAcademicYearByIdSpec(academicYearId), cancellationToken);
            return academicYear != null;
        }

        private async Task<bool> CurriculumBelongsToCourse(
            BulkInitializeClassSectionsForAcademicYear.Payload payload,
            CancellationToken cancellationToken)
        {
            var curriculum = await _curriculumRepository
                .FirstOrDefaultAsync(new GetCurriculumByIdSpec(payload.curriculumId), cancellationToken);

            if (curriculum == null) return false;

            return curriculum.CourseId == payload.courseId;
        }
    }
}
