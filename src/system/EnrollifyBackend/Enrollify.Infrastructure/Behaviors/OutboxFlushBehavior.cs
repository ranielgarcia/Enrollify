using Ardalis.Result;
using MediatR;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.Infrastructure.Behaviors;

/// <summary>
/// Automatically flushes any messages published through the enrolled <see cref="IDbContextOutbox"/>
/// (via <see cref="Enrollify.SharedKernel.IDomainEventBus"/>,
/// <see cref="Enrollify.Core.Services.NotificationServices.INotificationPublisher"/>, or
/// <see cref="Enrollify.Core.Services.NotificationServices.INotificationBus"/>) right after a request
/// handler completes successfully.
/// <para>
/// This is a safety net, not a replacement for explicit transaction management. Handlers that need
/// several repository writes plus published messages to succeed/fail atomically should still use
/// <see cref="Enrollify.SharedKernel.IUnitOfWork.BeginTransactionAsync(CancellationToken)"/> and call
/// <see cref="Enrollify.SharedKernel.IUnitOfWork.SaveChangesAndFlushMessagesThenCommitAsync(CancellationToken)"/>
/// themselves so the DbContext SaveChanges, the ambient relational transaction commit, and the outbox
/// flush all happen together (see <c>DbContextOutbox.SaveChangesAndFlushMessagesAsync</c>, which commits
/// <c>DbContext.Database.CurrentTransaction</c> when one is open, then flushes outgoing messages).
/// </para>
/// <para>
/// Without this behavior, a handler that injects <c>IDomainEventBus</c>/<c>INotificationPublisher</c> but
/// never explicitly calls <c>SaveChangesAndFlushMessagesThenCommitAsync</c> (or that commits its own
/// transaction directly instead of going through <see cref="Enrollify.SharedKernel.IUnitOfWork"/>) will
/// silently queue messages in the outbox that are never persisted/sent - the entity writes succeed, but
/// the published event/notification is lost. This behavior closes that gap for every request by default.
/// </para>
/// <para>
/// Registered as the innermost pipeline behavior (last one added in <c>MediatorConfig</c>) so it wraps
/// directly around the handler and runs immediately after <c>Handle</c> returns. It only flushes when the
/// handler's response indicates success - for <see cref="Result"/>/<see cref="Result{T}"/> responses this
/// means <c>IsSuccess</c> is true; any other response type is treated as success since the handler
/// completed without throwing. On failure, nothing is flushed, so messages queued before the failure
/// simply stay unflushed (they were never persisted, so there is nothing to roll back).
/// </para>
/// Calling <see cref="IDbContextOutbox.SaveChangesAndFlushMessagesAsync(CancellationToken)"/> again after a
/// handler already called it explicitly (e.g. via <c>SaveChangesAndFlushMessagesThenCommitAsync</c>) is
/// safe: there are no pending DbContext changes and no open transaction left to act on, and the outbox has
/// already cleared its outgoing message buffer, so the extra call is a harmless no-op.
/// </summary>
public sealed class OutboxFlushBehavior<TRequest, TResponse>(IDbContextOutbox outbox)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    TResponse response = await next();

    if (IsSuccess(response))
      await outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);

    return response;
  }

  private static bool IsSuccess(TResponse response)
  {
    // Treat any non-Ardalis.Result response as successful - the handler completed without throwing.
    return response is not Result result || result.IsSuccess;
  }
}
