using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoleConfigs;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("Name");
        //.HasMaxLength(RoleName.MaxLength)
        //.HasColumnType($"VARCHAR({RoleName.MaxLength})");

        builder.Property(e => e.Description)
            .HasColumnName("Description");
            //.HasMaxLength(RoleDescription.MaxLength)
            //.HasColumnType($"VARCHAR({RoleDescription.MaxLength})");

        // Audit fields
        builder.ConfigureAuditFields();

        builder.Navigation(r => r.RolePermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<RolePermission>(r => r.RolePermissions, rp =>
        {
            rp.ToTable("RolePermissions");

            rp.HasKey(e => new {e.RoleId, e.PermissionId });

            rp.WithOwner().HasForeignKey(e => e.RoleId);
            rp.Property(e => e.PermissionId)
              .IsRequired();
            rp.ConfigureAuditFields();
        });

    }
}
