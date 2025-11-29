using Enrollify.Core.Aggregates.PermissionsAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RolePermissionConfigs;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");


        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(PermissionName.MaxLength)
            .HasColumnType($"VARCHAR({PermissionName.MaxLength})");

        builder.Property(e => e.Resource)
            .HasMaxLength(PermissionResource.MaxLength)
            .HasColumnType($"VARCHAR({PermissionResource.MaxLength})");

        builder.Property(e => e.Action)
            .HasMaxLength(PermissionAction.MaxLength)
            .HasColumnType($"VARCHAR({PermissionAction.MaxLength})");

        // Audit fields
        builder.ConfigureAuditFields();
    }
}
