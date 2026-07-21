using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.NotificationConfigs;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
  public void Configure(EntityTypeBuilder<Notification> builder)
  {
    builder.ToTable("Notifications");

    builder.HasKey(e => e.Id);
    builder.Property(e => e.Id)
      .UseIdentityColumn()
      .IsRequired();

    builder.Property(e => e.Type).IsRequired();
    builder.Property(e => e.Title).IsRequired();
    builder.Property(e => e.Message).IsRequired();

    builder.Property(e => e.Severity)
      .HasConversion(
        v => v.Name,
        v => NotificationSeverityEnum.FromName(v))
      .HasColumnType("NVARCHAR(20)")
      .IsRequired();

    builder.Property(e => e.Category)
      .HasConversion(
        v => v.Name,
        v => NotificationCategoryEnum.FromName(v))
      .HasColumnType("NVARCHAR(50)")
      .IsRequired();

    builder.Property(e => e.ReferenceType)
      .HasConversion(
        v => v.Name,
        v => NotificationReferenceTypeEnum.FromName(v))
      .HasColumnType("NVARCHAR(100)")
      .IsRequired(false);

    builder.Property(e => e.ReferenceId).IsRequired(false);

    builder.Property(e => e.TargetScope)
      .HasConversion(
        v => v.Name,
        v => NotificationTargetScopeEnum.FromName(v))
      .HasColumnType("NVARCHAR(20)")
      .IsRequired();

    builder.Property(e => e.TargetUserId).IsRequired(false);
    builder.Property(e => e.TargetRoleId).IsRequired(false);
    builder.Property(e => e.RetentionDays).IsRequired().HasDefaultValue(NotificationSettings.DefaultRetentionDays);
    builder.Property(e => e.ExpiresAt).IsRequired(false);


    // Audit fields
    builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt");

    // Foreign key relationships for audit fields
    builder.HasOne(e => e.CreatedByUser)
      .WithMany()
      .HasForeignKey(e => e.CreatedBy)
      .OnDelete(DeleteBehavior.NoAction);

    // EF Core can't add/remove items via a read-only collection,
    // so you tell EF to use the backing field instead of the property
    // This is mainly about materialization and change-tracking without requiring a public setter or a mutable collection property.
    builder.Navigation(c => c.Recipients)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.OwnsMany(n => n.Recipients, a =>
    {
      a.ToTable("NotificationRecipients");

      a.WithOwner().HasForeignKey(e => e.NotificationId);

      a.HasKey(e => e.Id);
      a.Property(e => e.Id)
        .UseIdentityColumn()
        .IsRequired();

      a.Property(e => e.NotificationId).IsRequired();
      a.Property(e => e.UserId).IsRequired();
      a.Property(e => e.IsRead).IsRequired().HasDefaultValue(false);
      a.Property(e => e.ReadAt).IsRequired(false);
      a.Property(e => e.IsDismissed).IsRequired().HasDefaultValue(false);
      a.Property(e => e.DismissedAt).IsRequired(false);
    });
  }
}
