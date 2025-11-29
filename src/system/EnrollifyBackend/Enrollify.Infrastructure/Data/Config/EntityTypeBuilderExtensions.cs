using Enrollify.Core;

namespace Enrollify.Infrastructure.Data.Config;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable<AuditInfo>
    {
        builder.ComplexProperty(e => e.AuditInfo, auditBuilder =>
        {
            auditBuilder.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            auditBuilder.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            auditBuilder.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            auditBuilder.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            auditBuilder.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            auditBuilder.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            auditBuilder.Property(a => a.IsActive).HasColumnName("IsActive");
        });
    }

    public static void ConfigureAuditFields<TOwner, TEntity>(this OwnedNavigationBuilder<TOwner, TEntity> builder)
        where TOwner : class
        where TEntity : class, IAuditable<AuditInfo>
    {
        builder.OwnsOne(e => e.AuditInfo, auditBuilder =>
        {
            auditBuilder.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            auditBuilder.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            auditBuilder.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            auditBuilder.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            auditBuilder.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            auditBuilder.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            auditBuilder.Property(a => a.IsActive).HasColumnName("IsActive");
        });
    }
}