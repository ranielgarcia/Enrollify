using Enrollify.Core;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;

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
