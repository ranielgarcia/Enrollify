using Ardalis.Result;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Mediator;

namespace Enrollify.Application.Curriculums.Features;

public static class CreateDraftCurricula
{
    public sealed record Command(CourseId courseId, int effectiveYear, string version, string? description) :
        ICommand<Result<CurriculumId>>;

    public sealed class Handler : ICommandHandler<Command, Result<CurriculumId>>
    {
        private readonly ICurriculumRepository _curriculumRepository;

        public Handler(ICurriculumRepository curriculumRepository)
        {
            _curriculumRepository = curriculumRepository;
        }

        public async ValueTask<Result<CurriculumId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var draftCurriculumForCreation = new DraftCurriculumForCreation
            {
                CourseId = command.courseId,
                EffectiveYear = command.effectiveYear,
                Version = command.version,
                Description = command.description
            };

            var newDraftCurriculum = Curriculum.CreateDraftCurriculum(draftCurriculumForCreation);

            var result = await _curriculumRepository.CreateDraftCurricula(newDraftCurriculum, cancellationToken);
            return result;
        }
    }
}
