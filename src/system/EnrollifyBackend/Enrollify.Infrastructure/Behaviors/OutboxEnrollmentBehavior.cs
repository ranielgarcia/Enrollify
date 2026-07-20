using Enrollify.Infrastructure.Data;
using Enrollify.SharedKernel;
using MediatR;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.Infrastructure.Behaviors;

/// <summary>
/// Enrolls the scoped <see cref="EnrollifyDbContext"/> into the scoped <see cref="IDbContextOutbox"/>
/// before every MediatR request is handled.
/// <para>
/// This mirrors what Wolverine does automatically for its own handlers: before invoking the handler,
/// Wolverine enrolls the DbContext used by that unit of work into the outbox so any message published
/// during handling is transactionally durable. Since this codebase uses MediatR (not native Wolverine
/// handlers), <see cref="IDomainEventBus"/>, <see cref="Enrollify.Core.Services.NotificationServices.INotificationPublisher"/>,
/// and <see cref="Enrollify.Core.Services.NotificationServices.INotificationBus"/> all inject the same
/// scoped <see cref="IDbContextOutbox"/> instance as <see cref="IUnitOfWork"/>. Without this behavior,
/// publishing before <see cref="IUnitOfWork.BeginTransactionAsync(CancellationToken)"/> is called would
/// use an outbox that hasn't been enrolled yet.
/// </para>
/// <see cref="IDbContextOutbox.Enroll(Microsoft.EntityFrameworkCore.DbContext)"/> is idempotent (it only
/// assigns a field), so calling it here on every request - and again defensively inside
/// <see cref="EfUnitOfWork.BeginTransactionAsync(CancellationToken)"/> - is safe.
/// </summary>
public sealed class OutboxEnrollmentBehavior<TRequest, TResponse>(
  EnrollifyDbContext dbContext,
  IDbContextOutbox outbox)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    outbox.Enroll(dbContext);

    return next();
  }
}
