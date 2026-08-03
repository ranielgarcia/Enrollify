namespace Enrollify.Application;

/// <summary>
/// Single entry point for publishing events from application command/query handlers.
/// Use <see cref="DispatchAsync"/> for in-process events handled synchronously via MediatR
/// (completes within the current HTTP request).
/// Use <see cref="DispatchDeferredAsync"/> for events that must be processed in the background
/// via Wolverine's durable outbox (survives process restart, runs after HTTP response).
///
/// Never inject <c>IMediator</c> or <c>IDbContextOutbox</c> directly in command/query handlers
/// for event publishing — use this interface instead.
/// </summary>
public interface IApplicationEventDispatcher
{
  /// <summary>
  /// Publishes the event immediately via MediatR. Runs within the current HTTP request.
  /// Use for events that must be handled synchronously before the response is returned.
  /// </summary>
  Task DispatchAsync(IApplicationEvent applicationEvent, CancellationToken ct = default);

  /// <summary>
  /// Enqueues the event in the Wolverine durable outbox for background processing.
  /// Committed atomically with the current DB transaction; handled after the HTTP response is sent.
  /// Use for events whose handlers can run outside the current request lifetime.
  /// </summary>
  Task DispatchDeferredAsync(IApplicationEvent applicationEvent, CancellationToken ct = default);
}
