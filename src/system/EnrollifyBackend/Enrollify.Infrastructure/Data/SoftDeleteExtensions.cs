using Enrollify.Core;
using Enrollify.Core.Authentication;
using Enrollify.Core.Constants;
using System.Linq.Expressions;
using Enrollify.Core.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Enrollify.Infrastructure.Data;

public static class SoftDeleteExtensions
{
  public static void ApplySoftDelete<TContext>(this TContext context, UserContext? currentUser)
    where TContext : DbContext
  {
    ChangeTracker changeTracker = context.ChangeTracker;

    foreach (EntityEntry entry in changeTracker.Entries())
      if (entry.Entity is IAuditable)
      {
        // Fall back to the seeded system user (Id=1) for background/startup
        // operations that run without an authenticated HTTP request context.
        UserId auditUserId = currentUser?.Id ?? SystemUserConstants.SystemUserId;

        switch (entry.State)
        {
          case EntityState.Added:

            entry.CurrentValues[nameof(IAuditable.IsActive)] = true;
            entry.CurrentValues[nameof(IAuditable.CreatedBy)] = auditUserId;
            entry.CurrentValues[nameof(IAuditable.CreatedAt)] = DateTimeOffset.UtcNow;
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

  public static void AddSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
  {
    foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
    {
      // Only apply once per root type; derived types inherit the filter.
      if (entityType.BaseType is not null) continue;

      // EF Core does not support query filters on owned entity types.
      // Owned collections are instead filtered at the domain level via their
      // navigation properties (exposing only active items).
      if (entityType.IsOwned()) continue;

      if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType)) continue;

      // Build expression: (e) => e.IsActive == true
      ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
      MemberExpression property = Expression.Property(parameter, nameof(IAuditable.IsActive));
      ConstantExpression isActive = Expression.Constant(true);
      BinaryExpression equal = Expression.Equal(property, isActive);
      LambdaExpression lambda = Expression.Lambda(equal, parameter);

      modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
    }
  }
}
