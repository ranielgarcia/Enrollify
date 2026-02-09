using Ardalis.Result;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.SubjectEquivalences;

public interface ISubjectEquivalenceGroupRepository
{
    Task<Result<SubjectEquivalenceGroupId>> AddNewSubjectEquivalenceGroup(SubjectEquivalenceGroup newGroup, CancellationToken cancellationToken);
    Task<Result<SubjectEquivalenceGroupId>> UpdateSubjectEquivalenceGroup(SubjectEquivalenceGroup updatedGroup, CancellationToken cancellationToken);
    Task<Result> Delete(SubjectEquivalenceGroupId Id, CancellationToken cancellationToken);
}
