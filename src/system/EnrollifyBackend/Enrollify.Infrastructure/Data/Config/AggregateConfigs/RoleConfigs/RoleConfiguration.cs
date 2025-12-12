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

        builder.Navigation(r => r.RolePermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);


        builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
        builder.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
        builder.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
        builder.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
        builder.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
        builder.Property(a => a.IsActive).HasColumnName("IsActive");

        // Foreign key relationships for audit fields
        builder.HasOne(e => e.CreatedByUser)
          .WithMany()
          .HasForeignKey(e => e.CreatedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.UpdatedByUser)
          .WithMany()
          .HasForeignKey(e => e.UpdatedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.DeletedByUser)
          .WithMany()
          .HasForeignKey(e => e.DeletedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.OwnsMany<RolePermission>(r => r.RolePermissions, rp =>
        {
            rp.ToTable("RolePermissions");

            rp.WithOwner().HasForeignKey(e => e.RoleId);
            
            rp.HasKey(e => new { e.RoleId, e.PermissionScopeId });
            
            rp.Property(e => e.PermissionScopeId)
              .IsRequired();

            rp.Property(e => e.BitmaskPermission)
                .IsRequired();

            rp.Ignore(e => e.Permissions);
            rp.Ignore(e => e.PermissionScope);


            rp.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            rp.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            rp.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            rp.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            rp.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            rp.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            rp.Property(a => a.IsActive).HasColumnName("IsActive");

            // Foreign key relationships for audit fields
            rp.HasOne(e => e.CreatedByUser)
              .WithMany()
              .HasForeignKey(e => e.CreatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            rp.HasOne(e => e.UpdatedByUser)
              .WithMany()
              .HasForeignKey(e => e.UpdatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            rp.HasOne(e => e.DeletedByUser)
              .WithMany()
              .HasForeignKey(e => e.DeletedBy)
              .OnDelete(DeleteBehavior.NoAction);
        });



    }
}
