using Ardalis.Result;
using Enrollify.Application.Command.Persistence;
using Enrollify.Infrastructure.Data;
using Enrollify.Infrastructure.Persistence;

namespace Enrollify.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity, TId>
    where TEntity : class
{
    protected readonly EnrollifyDbContext DbContext;
    protected readonly IDbExceptionTranslator ExceptionTranslator;
    protected readonly ILogger Logger;

    protected RepositoryBase(
        EnrollifyDbContext dbContext,
        IDbExceptionTranslator exceptionTranslator,
        ILogger logger)
    {
        DbContext = dbContext;
        ExceptionTranslator = exceptionTranslator;
        Logger = logger;
    }

    protected async Task<Result<TId>> ExecuteCreateAsync(
        TEntity entity,
        Func<TEntity, TId> getId,
        Func<PersistenceError, Result<TId>>? onError,
        CancellationToken ct)
    {
        try
        {
            await DbContext.Set<TEntity>().AddAsync(entity, ct);
            await DbContext.SaveChangesAsync(ct);
            return Result.Success(getId(entity));
        }
        catch (DbUpdateException ex)
        {
            var error = ExceptionTranslator.Translate(ex);
            if (error is not null && onError is not null)
                return onError(error);

            Logger.LogError(ex, "Unhandled persistence error creating {Entity}", typeof(TEntity).Name);
            return Result.Error("An unexpected error occurred.");
        }
    }

    protected async Task<Result<TId>> ExecuteUpdateAsync(
        TEntity entity,
        Func<TEntity, TId> getId,
        Func<PersistenceError, Result<TId>>? onError,
        CancellationToken ct)
    {
        try
        {
            DbContext.Set<TEntity>().Update(entity);
            await DbContext.SaveChangesAsync(ct);
            return Result.Success(getId(entity));
        }
        catch (DbUpdateException ex)
        {
            var error = ExceptionTranslator.Translate(ex);
            if (error is not null && onError is not null)
                return onError(error);

            Logger.LogError(ex, "Unhandled persistence error updating {Entity}", typeof(TEntity).Name);
            return Result.Error("An unexpected error occurred.");
        }
    }

    protected async Task<Result> ExecuteDeleteAsync(
        TId id,
        Func<IQueryable<TEntity>, TId, CancellationToken, Task<TEntity?>> findEntity,
        Func<PersistenceError, Result>? onError,
        CancellationToken ct)
    {
        try
        {
            var entity = await findEntity(DbContext.Set<TEntity>().AsQueryable(), id, ct);
            if (entity is null)
                return Result.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");

            DbContext.Set<TEntity>().Remove(entity);
            await DbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            var error = ExceptionTranslator.Translate(ex);
            if (error is not null && onError is not null)
                return onError(error);

            Logger.LogError(ex, "Unhandled persistence error deleting {Entity} with ID {Id}", typeof(TEntity).Name, id);
            return Result.Error("An unexpected error occurred.");
        }
    }
}
