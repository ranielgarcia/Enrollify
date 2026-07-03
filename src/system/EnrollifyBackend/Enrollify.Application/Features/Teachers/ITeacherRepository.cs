namespace Enrollify.Application.Features.Teachers;

public interface ITeacherRepository
{
    Task<Result<TeacherId>> Create (Teacher newTeacher, CancellationToken cancellationToken);
    Task<Result<TeacherId>> Update(Teacher newTeacher, CancellationToken cancellationToken);
    Task<Result> Delete (TeacherId id, CancellationToken cancellationToken);
}
