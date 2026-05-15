using Ardalis.Result;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.ClassSections.Commands;

public static class BulkInitializeClassSectionsForAcademicYear
{
    public sealed record Payload(
        CourseId courseId,
        AcademicYearId academicYearId,
        CurriculumId curriculumId,
        int numberOfSections);

    public sealed record Command (
        List<Payload> requestPayload
        ) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IReadRepository<Course> _courseRepository;
        private readonly IReadRepository<AcademicYear> _academicYearRepository;
        private readonly IReadRepository<Curriculum> _curriculumRepository;

        public Handler(
            IReadRepository<Course> courseRepository,
            IReadRepository<AcademicYear> academicYearRepository,
            IReadRepository<Curriculum> curriculumRepository)
        {
            _courseRepository = courseRepository;
            _academicYearRepository = academicYearRepository;
            _curriculumRepository = curriculumRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {

            return Result.Success();
        }
    }

}
