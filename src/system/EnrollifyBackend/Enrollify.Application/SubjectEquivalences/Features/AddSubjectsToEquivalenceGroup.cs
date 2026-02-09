using Ardalis.Result;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.SubjectEquivalences.Features;

public static class AddSubjectsToEquivalenceGroup
{
    public sealed record Command(SubjectEquivalenceGroupId groupId, List<SubjectCode> subjectCodes) : ICommand<Result<SubjectEquivalenceGroupId>>;

    public sealed class Handler : ICommandHandler<Command, Result<SubjectEquivalenceGroupId>>
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
        public async ValueTask<Result<SubjectEquivalenceGroupId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var groupToUpdate = await _readRepository.GetByIdAsync(command.groupId, cancellationToken);
            if (groupToUpdate is null)
            {
                return Result.NotFound();
            }

            var subjects = await _subjectReadRepository.ListAsync(new ListSubjectsByCodesSpec(command.subjectCodes), cancellationToken);

            var missingSubjects = command.subjectCodes.Except(subjects.Select(s => s.Code)).ToList();
            if (missingSubjects.Any())
            {
                return Result.Invalid(new ValidationError($"Subjects with codes {string.Join(", ", missingSubjects)} not found"));
            }

            foreach (var code in command.subjectCodes)
            {
                var subject = subjects.FirstOrDefault(s => s.Code == code);
                if (subject is null)
                {
                    return Result.NotFound($"Subject with code {code} not found.");
                }
                groupToUpdate.AddSubject(subject.Id);
            }
            var result = await _repository.UpdateSubjectEquivalenceGroup(groupToUpdate, cancellationToken);
            return result;
        }
    }
}
