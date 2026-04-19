using Ardalis.Result;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Mediator;

namespace Enrollify.Application.Features.SubjectEquivalences.Commands;

public static class DeleteSubjectEquivalenceGroup
{
    public sealed record Command(SubjectEquivalenceGroupId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly ISubjectEquivalenceGroupRepository _repository;
        public Handler(ISubjectEquivalenceGroupRepository repository)
        {
            _repository = repository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var result = await _repository.Delete(command.id, cancellationToken);
            return result;
        }
    }
}
