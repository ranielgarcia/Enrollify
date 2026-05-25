using Ardalis.Result;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.SubjectEquivalences.Commands;

public static class RemoveSubjectFromEquivalenceGroup
{
    public sealed record Command (SubjectEquivalenceGroupId groupId, SubjectCode subjectCode) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ISubjectEquivalenceGroupRepository _repository;
        private readonly IReadRepository<SubjectEquivalenceGroup> _readRepository;
        private readonly IReadRepository<Subject> _subjectReadRepository;
        public Handler(ISubjectEquivalenceGroupRepository repository, IReadRepository<SubjectEquivalenceGroup> readRepository, IReadRepository<Subject> subjectReadRepository)
        {
            _repository = repository;
            _readRepository = readRepository;
            _subjectReadRepository = subjectReadRepository;
        }
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var groupToUpdate = await _readRepository.GetByIdAsync(command.groupId, cancellationToken);
            if (groupToUpdate is null)
            {
                return Result.NotFound();
            }
            var subject = await _subjectReadRepository.ListAsync(new ListMinimumSubjectsByCodesSpec(new List<SubjectCode> { command.subjectCode }), cancellationToken);
            if (subject is null || !subject.Any())
            {
                return Result.NotFound($"Subject with code {command.subjectCode} not found.");
            }
            groupToUpdate.RemoveSubject(subject.First().Id);
            await _repository.UpdateSubjectEquivalenceGroup(groupToUpdate, cancellationToken);
            return Result.Success();
        }
    }
}
