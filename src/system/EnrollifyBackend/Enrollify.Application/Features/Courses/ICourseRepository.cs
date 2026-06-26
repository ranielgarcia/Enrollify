namespace Enrollify.Application.Features.Courses;

public interface ICourseRepository
{
    Task<Result<CourseId>> Create(Course newCourse, CancellationToken cancellationToken);
    Task<Result<CourseId>> Update(Course newCourse, CancellationToken cancellationToken);
    Task<Result> Delete (CourseId id, CancellationToken cancellationToken);
}
