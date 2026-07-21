namespace Enrollify.SharedKernel;

/// <summary>
/// Provides transaction management for coordinating multiple repository operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transaction scope that can be committed or rolled back.</returns>
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Begins a new database transaction, with specific isolation level
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transaction scope that can be committed or rolled back.</returns>
    Task<ITransactionScope> BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits all changes and flushes persisted messages
    /// to the persistent outbox
    /// in the correct order
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAndFlushMessagesThenCommitAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a database transaction that can be committed or rolled back.
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// Commits all changes made in the transaction to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back all changes made in the transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
