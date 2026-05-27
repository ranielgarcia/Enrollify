using Ardalis.Result;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using MediatR;

namespace Enrollify.Application.Features.SubjectEquivalences.Commands;

public static class AddNewSubjectEquivalenceGroup
{
    public sealed record Command(string name) : IRequest<Result<SubjectEquivalenceGroupId>>;

    public sealed class Handler : IRequestHandler<Command, Result<SubjectEquivalenceGroupId>>
    {
        private readonly ISubjectEquivalenceGroupRepository _repository;
        public Handler(ISubjectEquivalenceGroupRepository repository)
        {
            _repository = repository;
        }
        public async Task<Result<SubjectEquivalenceGroupId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var newGroup = new SubjectEquivalenceGroup(command.name);
            var result = await _repository.AddNewSubjectEquivalenceGroup(newGroup, cancellationToken);
            return result;
        }
    }
}
