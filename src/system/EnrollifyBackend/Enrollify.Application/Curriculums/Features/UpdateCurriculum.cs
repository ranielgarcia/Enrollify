using Ardalis.Result;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Curriculums.Features;

public static class UpdateCurriculum
{
    public sealed record Command(CurriculumId Id, CourseId courseId, int effectiveYear, string version, string? description) :
        ICommand<Result<CurriculumId>>;

    public sealed class Handler : ICommandHandler<Command, Result<CurriculumId>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IReadRepository<Curriculum> _curriculumReadRepository;
        private readonly IReadRepository<Course> _courseReadRepository;

        public Handler(
            ICurriculumRepository curriculumRepository,
            IReadRepository<Curriculum> curriculumReadRepository,
            IReadRepository<Course> courseReadRepository)
        {
            _curriculumRepository = curriculumRepository;
            _curriculumReadRepository = curriculumReadRepository;
            _courseReadRepository = courseReadRepository;
        }

        public async ValueTask<Result<CurriculumId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var course = await _courseReadRepository.GetByIdAsync(command.courseId, cancellationToken);
            if (course == null)
                return Result.NotFound($"Course with an id of {command.courseId} not found");

            var curriculum = await _curriculumReadRepository.GetByIdAsync(command.Id, cancellationToken);
            if (curriculum == null)
                return Result.NotFound($"Curriculumn with an id of {command.Id} not found");

            // Status should be updated manually
            curriculum
                .UpdateCourse(command.courseId)
                .UpdateEffectiveYear(command.effectiveYear)
                .UpdateVersion(command.version)
                .UpdateDescription(command.description);

            var result = await _curriculumRepository.UpdateCurriculum(curriculum, cancellationToken);
            return result;
        }
    }
}
