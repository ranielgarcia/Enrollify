using MediatR;

namespace Enrollify.SharedKernel;

/// <summary>
/// Marker interface for events published explicitly from application command/query handlers.
/// Implemented by both <see cref="DomainEventBase"/> (aggregate-raised events dispatched via outbox)
/// and <see cref="T:Enrollify.Application.ApplicationEventBase"/> (application-layer events).
/// Use <see cref="T:Enrollify.Application.IApplicationEventDispatcher"/> to publish — never inject
/// <c>IMediator</c> or <c>IDbContextOutbox</c> directly in handlers for event publishing.
/// </summary>
public interface IApplicationEvent : INotification
{
  DateTime DateOccurred { get; }
}
