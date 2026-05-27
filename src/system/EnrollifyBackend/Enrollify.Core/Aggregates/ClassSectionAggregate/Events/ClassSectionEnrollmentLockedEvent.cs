using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

public class ClassSectionEnrollmentLockedEvent(ClassSectionId id) : DomainEventBase
{
    public ClassSectionId Id { get; init; } = id;
}
