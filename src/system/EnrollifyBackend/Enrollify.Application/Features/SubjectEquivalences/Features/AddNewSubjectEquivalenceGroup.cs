using Ardalis.Result;
using Enrollify.Application.Features.SubjectEquivalences;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Mediator;

namespace Enrollify.Application.Features.SubjectEquivalences.Features;

public static class AddNewSubjectEquivalenceGroup
{
    public sealed record Command(string name) : ICommand<Result<SubjectEquivalenceGroupId>>;

    public sealed class Handler : ICommandHandler<Command, Result<SubjectEquivalenceGroupId>>
    {
        private readonly ISubjectEquivalenceGroupRepository _repository;
        public Handler(ISubjectEquivalenceGroupRepository repository)
        {
            _repository = repository;
        }
        public async ValueTask<Result<SubjectEquivalenceGroupId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var newGroup = new SubjectEquivalenceGroup(command.name);
            var result = await _repository.AddNewSubjectEquivalenceGroup(newGroup, cancellationToken);
            return result;
        }
    }
}
