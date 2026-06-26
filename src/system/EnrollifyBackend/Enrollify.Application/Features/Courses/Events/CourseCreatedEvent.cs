namespace Enrollify.Application.Features.Courses.Events;

public sealed class CourseCreatedEvent (CourseId courseId) : DomainEventBase
{
    public CourseId CourseId { get; init; } = courseId;
}
