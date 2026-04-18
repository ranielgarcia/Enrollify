using Ardalis.Result;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.Subjects;

public interface ISubjectRepository
{
    Task<Result<SubjectId>> Create(Subject newSubject, CancellationToken cancellationToken);
    Task<Result<SubjectId>> Update(Subject newSubject, CancellationToken cancellationToken);
    Task<Result> Delete(SubjectId id, CancellationToken cancellationToken);
}
