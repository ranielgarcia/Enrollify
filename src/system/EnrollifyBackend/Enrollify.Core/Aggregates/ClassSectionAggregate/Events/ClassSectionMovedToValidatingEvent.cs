using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

public class ClassSectionMovedToValidatingEvent(ClassSectionId id) : DomainEventBase
{
  public ClassSectionId Id { get; init; } = id;
}

