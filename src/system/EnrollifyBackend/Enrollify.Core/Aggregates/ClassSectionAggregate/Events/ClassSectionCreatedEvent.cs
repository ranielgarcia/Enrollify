using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

/// <summary>Raised after a ClassSection is first persisted so the eligibility projection can be initialised.</summary>
public class ClassSectionCreatedEvent(ClassSectionId id, UserId? triggeredBy = null) : DomainEventBase
{
  public ClassSectionId Id { get; init; } = id;

  /// <summary>
  /// The user who triggered the creation. Populated when published from an HTTP request context;
  /// null for system-initiated paths where no user is authenticated.
  /// </summary>
  public UserId? TriggeredBy { get; init; } = triggeredBy;
}

