using Ardalis.Result;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.SubjectEquivalences.Commands;

public static class UpdateSubjectEquivalenceGroup
{
    public sealed record Command(SubjectEquivalenceGroupId id, string name) : IRequest<Result<SubjectEquivalenceGroupId>>;

    public sealed class Handler : IRequestHandler<Command, Result<SubjectEquivalenceGroupId>>
    {
        private readonly ISubjectEquivalenceGroupRepository _repository;
        private readonly IReadRepository<SubjectEquivalenceGroup> _readRepository;

        public Handler(ISubjectEquivalenceGroupRepository repository, IReadRepository<SubjectEquivalenceGroup> readRepository)
        {
            _repository = repository;
            _readRepository = readRepository;
        }
        public async Task<Result<SubjectEquivalenceGroupId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var groupToUpdate = await _readRepository.GetByIdAsync(command.id, cancellationToken);
            if (groupToUpdate is null)
            {
                return Result.NotFound();
            }

            groupToUpdate.UpdateName(command.name);

            var result = await _repository.UpdateSubjectEquivalenceGroup(groupToUpdate, cancellationToken);
            return result;
        }
    }
}
