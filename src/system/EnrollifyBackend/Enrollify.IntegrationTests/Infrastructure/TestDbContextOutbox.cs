using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Runtime;

namespace Enrollify.IntegrationTests.Infrastructure;

/// <summary>
/// No-op stub for <see cref="IDbContextOutbox"/> used in integration tests.
/// All message publishing calls are silently discarded so tests do not require
/// Wolverine's SQL Server persistence infrastructure.
/// <see cref="SaveChangesAndFlushMessagesAsync"/> delegates to the enrolled
/// <see cref="DbContext"/> so EF Core changes are still persisted correctly.
/// </summary>
internal sealed class TestDbContextOutbox : IDbContextOutbox
{
    private DbContext? _dbContext;

    // ── IDbContextOutbox ─────────────────────────────────────────────────────

    public void Enroll(DbContext dbContext) => _dbContext = dbContext;

    public async Task SaveChangesAndFlushMessagesAsync(CancellationToken cancellation = default)
    {
      if (_dbContext is not null)
      {
        await _dbContext.SaveChangesAsync(cancellation);
        if (_dbContext.Database.CurrentTransaction != null)
        {
            await _dbContext.Database.CurrentTransaction.CommitAsync(cancellation);
        }
      }
    }

    public async Task SaveChangesAndFlushMessagesAsync(MultiFlushMode mode, CancellationToken cancellation = default)
    {
      if (_dbContext is not null)
      {
        await _dbContext.SaveChangesAsync(cancellation);
        if (_dbContext.Database.CurrentTransaction != null)
        {
          await _dbContext.Database.CurrentTransaction.CommitAsync(cancellation);
        }
      }
    }

    public Task FlushOutgoingMessagesAsync() => Task.CompletedTask;

    public DbContext? ActiveContext => _dbContext;

    // ── IMessageBus ──────────────────────────────────────────────────────────

    public string? TenantId { get; set; }

    /// <summary>Silently discard the published message.</summary>
    public ValueTask PublishAsync<T>(T message, DeliveryOptions? options = null) => ValueTask.CompletedTask;

    /// <summary>Silently discard the sent message.</summary>
    public ValueTask SendAsync<T>(T message, DeliveryOptions? options = null) => ValueTask.CompletedTask;

    public ValueTask BroadcastToTopicAsync(string topicName, object message, DeliveryOptions? options = null) => ValueTask.CompletedTask;

    public IReadOnlyList<Envelope> PreviewSubscriptions(object message) => Array.Empty<Envelope>();

    public IReadOnlyList<Envelope> PreviewSubscriptions(object message, DeliveryOptions options) => Array.Empty<Envelope>();

    public IDestinationEndpoint EndpointFor(string endpointName)
        => throw new NotSupportedException("TestDbContextOutbox does not support EndpointFor.");

    public IDestinationEndpoint EndpointFor(Uri uri)
        => throw new NotSupportedException("TestDbContextOutbox does not support EndpointFor.");

    public Task InvokeForTenantAsync(string tenantId, object message, CancellationToken cancellation = default, TimeSpan? timeout = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support InvokeForTenantAsync.");

    public Task<T> InvokeForTenantAsync<T>(string tenantId, object message, CancellationToken cancellation = default, TimeSpan? timeout = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support InvokeForTenantAsync.");

    // ── ICommandBus ──────────────────────────────────────────────────────────

    public Task InvokeAsync(object message, CancellationToken cancellation = default, TimeSpan? timeout = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support InvokeAsync. Use IMediator.Send() instead.");

    public Task InvokeAsync(object message, DeliveryOptions options, CancellationToken cancellation = default, TimeSpan? timeout = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support InvokeAsync. Use IMediator.Send() instead.");

    public Task<T> InvokeAsync<T>(object message, CancellationToken cancellation = default, TimeSpan? timeout = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support InvokeAsync. Use IMediator.Send() instead.");

    public Task<T> InvokeAsync<T>(object message, DeliveryOptions options, CancellationToken cancellation = default, TimeSpan? timeout = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support InvokeAsync. Use IMediator.Send() instead.");

    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(object message, CancellationToken cancellation = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support StreamAsync.");

    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(object message, DeliveryOptions options, CancellationToken cancellation = default)
        => throw new NotSupportedException("TestDbContextOutbox does not support StreamAsync.");
}
