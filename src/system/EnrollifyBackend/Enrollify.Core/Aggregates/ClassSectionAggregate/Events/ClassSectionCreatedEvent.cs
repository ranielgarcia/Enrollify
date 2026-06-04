using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

/// <summary>Raised after a ClassSection is first persisted so the eligibility projection can be initialised.</summary>
public class ClassSectionCreatedEvent(ClassSectionId id) : DomainEventBase
{
  public ClassSectionId Id { get; init; } = id;
}

