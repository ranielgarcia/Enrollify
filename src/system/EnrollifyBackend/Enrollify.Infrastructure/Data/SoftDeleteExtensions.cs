using Enrollify.Core;
using Enrollify.Core.Authentication;
using System.Linq.Expressions;

namespace Enrollify.Infrastructure.Data;

public static class SoftDeleteExtensions
{
    public static void ApplySoftDelete<TContext>(this TContext context, UserContext? currentUser) where TContext : DbContext
    {
        var changeTracker = context.ChangeTracker;

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.Entity is IAuditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:

                        entry.CurrentValues[nameof(IAuditable.IsActive)] = true;

                        if (currentUser is not null)
                        {
                            entry.CurrentValues[nameof(IAuditable.CreatedBy)] = currentUser.Id;
                            entry.CurrentValues[nameof(IAuditable.CreatedAt)] = DateTimeOffset.UtcNow;
                        }
                        break;
                    case EntityState.Modified:

                        if (currentUser is not null)
                        {
                            entry.CurrentValues[nameof(IAuditable.UpdatedBy)] = currentUser.Id;
                            entry.CurrentValues[nameof(IAuditable.UpdatedAt)] = DateTimeOffset.UtcNow;
                        }
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;

                        entry.CurrentValues[nameof(IAuditable.IsActive)] = false;

                        if (currentUser is not null)
                        {
                            entry.CurrentValues[nameof(IAuditable.DeletedBy)] = currentUser.Id;
                            entry.CurrentValues[nameof(IAuditable.DeletedAt)] = DateTimeOffset.UtcNow;
                        }
                        break;
                }
            }
        }
    }

    public static void AddSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Only apply once per root type; derived types inherit the filter.
            if (entityType.BaseType is not null)
            {
                continue;
            }

            // EF Core does not support query filters on owned entity types.
            // Owned collections are instead filtered at the domain level via their
            // navigation properties (exposing only active items).
            if (entityType.IsOwned())
            {
                continue;
            }

            if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // Build expression: (e) => e.IsActive == true
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(IAuditable.IsActive));
            var isActive = Expression.Constant(true);
            var equal = Expression.Equal(property, isActive);
            var lambda = Expression.Lambda(equal, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

}
