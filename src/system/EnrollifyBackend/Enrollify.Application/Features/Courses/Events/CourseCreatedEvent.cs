using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Application.Features.Courses.Events;

public sealed class CourseCreatedEvent (CourseId courseId) : DomainEventBase
{
    public CourseId CourseId { get; init; } = courseId;
}
