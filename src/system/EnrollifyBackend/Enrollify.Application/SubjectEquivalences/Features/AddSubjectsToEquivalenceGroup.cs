using Ardalis.Result;
using Enrollify.Application.SubjectEquivalences.DTOs;
using Enrollify.Application.SubjectEquivalences.Specifications;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.SubjectEquivalences.Features;

public static class AddSubjectsToEquivalenceGroup
{
    public sealed record Command(SubjectEquivalenceGroupId groupId, List<SubjectId> subjectIds) : ICommand<Result<SubjectEquivalenceGroupDTO>>;

    public sealed class Handler : ICommandHandler<Command, Result<SubjectEquivalenceGroupDTO>>
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
        public async ValueTask<Result<SubjectEquivalenceGroupDTO>> Handle(Command command, CancellationToken cancellationToken)
        {
            var groupToUpdate = await _readRepository.FirstOrDefaultAsync(new GetSubjectEquivalenceGroupByIdWithSubjectsSpec(command.groupId), cancellationToken);
            if (groupToUpdate is null)
            {
                return Result.NotFound();
            }

            var subjects = await _subjectReadRepository.ListAsync(new ListSubjectsByIdsSpec(command.subjectIds), cancellationToken);

            var missingSubjects = command.subjectIds.Except(subjects.Select(s => s.Id)).ToList();
            if (missingSubjects.Any())
            {
                return Result.Invalid(new ValidationError($"Subjects with IDs {string.Join(", ", missingSubjects)} not found"));
            }

            foreach (var id in command.subjectIds)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == id);
                if (subject is null)
                {
                    return Result.NotFound($"Subject with ID {id} not found.");
                }
                groupToUpdate.AddSubject(subject.Id);
            }
            var result = await _repository.UpdateSubjectEquivalenceGroup(groupToUpdate, cancellationToken);

            return Result.Success(SubjectEquivalenceGroupDTO.FromEntity(groupToUpdate));
        }
    }
}
