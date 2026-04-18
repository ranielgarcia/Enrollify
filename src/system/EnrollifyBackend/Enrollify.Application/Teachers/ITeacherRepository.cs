using Ardalis.Result;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Teachers;

public interface ITeacherRepository
{
    Task<Result<TeacherId>> Create (Teacher newTeacher, CancellationToken cancellationToken);
    Task<Result<TeacherId>> Update(Teacher newTeacher, CancellationToken cancellationToken);
    Task<Result> Delete (TeacherId id, CancellationToken cancellationToken);
}
