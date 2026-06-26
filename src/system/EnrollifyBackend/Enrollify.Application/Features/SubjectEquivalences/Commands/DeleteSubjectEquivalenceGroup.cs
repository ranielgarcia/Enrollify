using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.Features.SubjectEquivalences.Commands;

public static class DeleteSubjectEquivalenceGroup
{
    public sealed record Command(SubjectEquivalenceGroupId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ISubjectEquivalenceGroupRepository _repository;
        public Handler(ISubjectEquivalenceGroupRepository repository)
        {
            _repository = repository;
        }
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var result = await _repository.Delete(command.id, cancellationToken);
            return result;
        }
    }
}
