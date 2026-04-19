using Ardalis.Result;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Curriculums.Commands;

public static class CreateDraftCurriculum
{
    public sealed record Command(CourseId courseId, int effectiveYear, string version, string? description) :
        ICommand<Result<CurriculumId>>;

    public sealed class Handler : ICommandHandler<Command, Result<CurriculumId>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IReadRepository<Course> _courseReadRepository;

        public Handler(ICurriculumRepository curriculumRepository, IReadRepository<Course> courseReadRepository)
        {
            _curriculumRepository = curriculumRepository;
            _courseReadRepository = courseReadRepository;
        }

        public async ValueTask<Result<CurriculumId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var course = await _courseReadRepository.GetByIdAsync(command.courseId, cancellationToken);
            if (course == null)
                return Result.NotFound($"Course with an id of {command.courseId.Value} not found");

            var draftCurriculumForCreation = new DraftCurriculumForCreation
            {
                CourseId = command.courseId,
                EffectiveYear = command.effectiveYear,
                Version = command.version,
                Description = command.description
            };

            var newDraftCurriculum = Curriculum.CreateDraftCurriculum(draftCurriculumForCreation);

            var result = await _curriculumRepository.CreateDraftCurriculum(newDraftCurriculum, cancellationToken);
            return result;
        }
    }
}
