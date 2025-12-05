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

        builder.Property(e => e.Description)
            .HasColumnName("Description");

        // Audit fields
        builder.ConfigureAuditFields();

        builder.Navigation(r => r.RolePermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<RolePermission>(r => r.RolePermissions, rp =>
        {
            rp.ToTable("RolePermissions");

            rp.WithOwner().HasForeignKey(e => e.RoleId);
            rp.Property(e => e.PermissionScopeId)
              .IsRequired();

            rp.Property(e => e.BitmaskPermission)
                .IsRequired();

            rp.ConfigureAuditFields();
        });

    }
}
