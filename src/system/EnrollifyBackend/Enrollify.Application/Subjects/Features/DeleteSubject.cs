using Ardalis.Result;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public static class DeleteSubject
{
    public sealed record Command(SubjectId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ISubjectRepository _subjectRepository;
        public Handler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            return await _subjectRepository.Delete(command.id, cancellationToken);
        }
    }
}
