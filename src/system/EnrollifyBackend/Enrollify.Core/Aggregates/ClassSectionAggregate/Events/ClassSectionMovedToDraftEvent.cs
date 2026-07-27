using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

public class ClassSectionMovedToDraftEvent(ClassSectionId id, UserId? triggeredBy = null) : DomainEventBase
{
  public ClassSectionId Id { get; init; } = id;

  /// <summary>
  /// The user whose action caused the section to move to Draft status.
  /// Null when triggered from a background process with no authenticated user.
  /// </summary>
  public UserId? TriggeredBy { get; init; } = triggeredBy;
}

