namespace Enrollify.Application;

/// <summary>
/// A base type for application events. Depends on Mediator INotification.
/// Includes DateOccurred which is set on creation.
/// </summary>
public class ApplicationEventBase : IApplicationEvent
{
  public DateTime DateOccurred { get; init; } = DateTime.UtcNow;
}
