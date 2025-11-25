using Enrollify.Core;

namespace Enrollify.Infrastructure.Data.Config;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable<AuditInfo>
    {
        builder.Property<DateTime>("CreatedAt")
            .HasColumnType("DATETIME2")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property<int>("CreatedBy")
            .IsRequired(true);

        builder.Property<DateTime?>("UpdatedAt")
            .HasColumnType("DATETIME2")
            .IsRequired(false);

        builder.Property<int?>("UpdatedBy")
            .IsRequired(false);

        builder.Property<DateTime?>("DeletedAt")
            .HasColumnType("DATETIME2")
            .IsRequired(false);

        builder.Property<int?>("DeletedBy")
            .IsRequired(false);

        builder.Property<bool>("IsActive")
            .HasDefaultValue(true)
            .IsRequired();

        builder.OwnsOne(e => e.AuditInfo, auditBuilder =>
        {
            auditBuilder.Property(a => a.CreatedAt)
              .HasColumnName("CreatedAt")
              .IsRequired();
            auditBuilder.Property(a => a.CreatedBy)
              .HasColumnName("CreatedBy");
            auditBuilder.Property(a => a.UpdatedAt)
              .HasColumnName("UpdatedAt");
            auditBuilder.Property(a => a.UpdatedBy)
              .HasColumnName("UpdatedBy");
            auditBuilder.Property(a => a.DeletedAt)
              .HasColumnName("DeletedAt");
            auditBuilder.Property(a => a.DeletedBy)
              .HasColumnName("DeletedBy");
            auditBuilder.Property(a => a.IsActive)
              .HasColumnName("IsActive")
              .IsRequired();
        });
    }
}