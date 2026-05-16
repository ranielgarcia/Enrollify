using Enrollify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Enrollify.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for database operations in integration tests.
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Clears all data from the specified entity table.
    /// WARNING: This is a destructive operation. Use with caution.
    /// </summary>
    public static async Task ClearTableAsync<TEntity>(EnrollifyDbContext dbContext) where TEntity : class
    {
        var entities = await dbContext.Set<TEntity>().ToListAsync();
        dbContext.Set<TEntity>().RemoveRange(entities);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a collection of entities into the database.
    /// </summary>
    public static async Task SeedEntitiesAsync<TEntity>(
        EnrollifyDbContext dbContext,
        params TEntity[] entities) where TEntity : class
    {
        await dbContext.Set<TEntity>().AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets an entity by its ID, or throws if not found.
    /// </summary>
    public static async Task<TEntity> GetByIdOrThrowAsync<TEntity>(
        EnrollifyDbContext dbContext,
        object id) where TEntity : class
    {
        var entity = await dbContext.Set<TEntity>().FindAsync(id);
        if (entity == null)
        {
            throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with ID {id} not found.");
        }
        return entity;
    }

    /// <summary>
    /// Checks if an entity exists by its ID.
    /// </summary>
    public static async Task<bool> ExistsAsync<TEntity>(
        EnrollifyDbContext dbContext,
        object id) where TEntity : class
    {
        var entity = await dbContext.Set<TEntity>().FindAsync(id);
        return entity != null;
    }

    /// <summary>
    /// Executes a raw SQL command against the database.
    /// </summary>
    public static async Task<int> ExecuteSqlAsync(
        EnrollifyDbContext dbContext,
        string sql,
        params object[] parameters)
    {
        return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    /// <summary>
    /// Gets the count of entities in a table.
    /// </summary>
    public static async Task<int> GetCountAsync<TEntity>(EnrollifyDbContext dbContext) where TEntity : class
    {
        return await dbContext.Set<TEntity>().CountAsync();
    }

    /// <summary>
    /// Detaches all entities from the DbContext change tracker.
    /// Useful for ensuring clean state between operations.
    /// </summary>
    public static void DetachAllEntities(EnrollifyDbContext dbContext)
    {
        var entries = dbContext.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Reloads an entity from the database, discarding any local changes.
    /// </summary>
    public static async Task<TEntity> ReloadEntityAsync<TEntity>(
        EnrollifyDbContext dbContext,
        TEntity entity) where TEntity : class
    {
        await dbContext.Entry(entity).ReloadAsync();
        return entity;
    }

    /// <summary>
    /// Executes an action within a database transaction that is automatically rolled back.
    /// Useful for testing without persisting changes.
    /// </summary>
    public static async Task ExecuteInTransactionAsync(
        EnrollifyDbContext dbContext,
        Func<Task> action)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await action();
            // Note: We don't commit - transaction is rolled back on dispose
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }
}
