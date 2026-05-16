using Enrollify.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage;

namespace Enrollify.Infrastructure.Data;

/// <summary>
/// EF Core implementation of Unit of Work pattern.
/// Wraps DbContext transaction management.
/// </summary>
internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly EnrollifyDbContext _dbContext;

    public EfUnitOfWork(EnrollifyDbContext dbContext)
    {
        _dbContext = dbContext;
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
