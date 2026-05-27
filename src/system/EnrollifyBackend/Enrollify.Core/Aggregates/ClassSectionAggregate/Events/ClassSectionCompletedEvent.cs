using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

public class ClassSectionCompletedEvent(ClassSectionId id) : DomainEventBase
{
    public ClassSectionId Id { get; init; } = id;
}
