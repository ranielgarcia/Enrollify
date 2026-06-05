using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

public class ClassSectionOpenedForEnrollmentEvent(ClassSectionId id) : DomainEventBase
{
    public ClassSectionId Id { get; init; } = id;
}
