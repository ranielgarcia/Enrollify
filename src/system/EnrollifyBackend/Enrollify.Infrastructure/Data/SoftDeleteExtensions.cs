using Enrollify.Core;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Enrollify.Infrastructure.Data;

public static class SoftDeleteExtensions
{
    public static void ApplySoftDelete<TContext>(this TContext context) where TContext : DbContext
    {
        var changeTracker = context.ChangeTracker;

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.Entity is IAuditable<AuditInfo>)
            {
                switch (entry.State)
                {
                    case EntityState.Added:

                        SetIsActive(entry, isActive: true);

                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        SetIsActive(entry, isActive: false);
                        SetDeletedAt(entry, DateTimeOffset.UtcNow);
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

            // Do not apply query filters to owned entity types (e.g., owned collections like RolePermission)
            // Owned types should be filtered/configured within their OwnsOne/OwnsMany mapping.
            if (entityType.IsOwned())
            {
                continue;
            }

            if (!typeof(IAuditable<AuditInfo>).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // Build expression: (e) => e.AuditInfo.IsActive == true
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var auditInfoProperty = Expression.Property(parameter, nameof(AuditInfo)); // property name on the entity is AuditInfo
            var isActiveProperty = Expression.Property(auditInfoProperty, nameof(AuditInfo.IsActive));
            var trueConstant = Expression.Constant(true);
            var body = Expression.Equal(isActiveProperty, trueConstant);

            var lambdaType = typeof(Func<,>).MakeGenericType(entityType.ClrType, typeof(bool));
            var lambda = Expression.Lambda(lambdaType, body, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private static void SetIsActive(EntityEntry entry, bool isActive)
    {
        // Complex property path (root entities)
        var complex = entry.Metadata.FindComplexProperty(nameof(AuditInfo));
        if (complex is not null)
        {
            entry.ComplexProperty(nameof(AuditInfo))
                 .Property(nameof(AuditInfo.IsActive))
                 .CurrentValue = isActive;
            return;
        }

        // The ComplexProperty(...) API is only defined on EntityTypeBuilder<TEntity>, not on OwnedNavigationBuilder<TOwner, TEntity>
        // Owned navigation path (owned entities like RolePermission)
        var ownedRef = entry.Reference(nameof(AuditInfo));
        var ownedEntry = ownedRef?.TargetEntry;
        if (ownedEntry is not null)
        {
            ownedEntry.Property(nameof(AuditInfo.IsActive)).CurrentValue = isActive;
        }
    }

    private static void SetDeletedAt(EntityEntry entry, DateTimeOffset utcNow)
    {
        var complex = entry.Metadata.FindComplexProperty(nameof(AuditInfo));
        if (complex is not null)
        {
            entry.ComplexProperty(nameof(AuditInfo))
                 .Property(nameof(AuditInfo.DeletedAt))
                 .CurrentValue = utcNow;
            return;
        }

        var ownedRef = entry.Reference(nameof(AuditInfo));
        var ownedEntry = ownedRef?.TargetEntry;
        if (ownedEntry is not null)
        {
            ownedEntry.Property(nameof(AuditInfo.DeletedAt)).CurrentValue = utcNow;
        }
    }
}
