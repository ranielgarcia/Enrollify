using Enrollify.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.Infrastructure.Data;

/// <summary>
/// EF Core implementation of Unit of Work pattern.
/// Wraps DbContext transaction management.
/// </summary>
internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly IDbContextOutbox _outbox;

    public EfUnitOfWork(EnrollifyDbContext dbContext, IDbContextOutbox outbox)
    {
        _dbContext = dbContext;
        _outbox = outbox;

        // attached the DBContext to the outbox, BEFORE sending any messages
        _outbox.Enroll(_dbContext);
    }

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionScope(transaction);
    }

    public async Task<ITransactionScope> BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return new EfTransactionScope(transaction);
    }

    // Only use this method if you are publishing messages/events using Wolverine infra
    // else, use the CommitAsync method of the ITransactionScope
    public async Task SaveChangesAndFlushMessagesThenCommitAsync(CancellationToken cancellationToken = default)
    {
      // Commit all changes and flush persisted messages
      // to the persistent outbox
      // in the correct order
      // Wolverine documentations: https://wolverinefx.net/guide/durability/efcore/outbox-and-inbox.html#transactional-inbox-and-outbox-with-ef-core
      await _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
    }
}

/// <summary>
/// Wraps an EF Core database transaction.
/// </summary>
internal sealed class EfTransactionScope : ITransactionScope
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    public EfTransactionScope(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
    }
}
