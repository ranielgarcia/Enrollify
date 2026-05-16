using Ardalis.Result;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Application.Features.Subjects;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Subjects.Commands;

public static class DeleteSubject
{
    public sealed record Command(SubjectId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReadRepository<Curriculum> _curriculumReadRepository;

        public Handler(ISubjectRepository subjectRepository, IReadRepository<Curriculum> curriculumReadRepository)
        {
            _subjectRepository = subjectRepository;
            _curriculumReadRepository = curriculumReadRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var curriculums = await _curriculumReadRepository.ListAsync(new GetCurriculumBySubjectIdSpec(command.id), cancellationToken);

            if (curriculums.Any())
            {
                var curriculumNames = string.Join(", ", curriculums.Select(c => $"{c.Description} ({c.Version})"));
                return Result.Forbidden($"Cannot delete subject because it is associated with one or more curriculums. [{curriculumNames}]");
            }

            return await _subjectRepository.Delete(command.id, cancellationToken);
        }
    }
}
