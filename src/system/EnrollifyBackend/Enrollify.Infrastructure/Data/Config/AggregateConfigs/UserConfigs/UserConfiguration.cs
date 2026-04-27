using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.UserConfigs;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.FirstName)
            //.HasColumnName("FirstName")
          .IsRequired();

        builder.Property(e => e.LastName)
            //.HasColumnName("LastName")
          .IsRequired();

        builder.Property(e => e.Email)
            //.HasColumnName("Email")
          .IsRequired();

        builder.Property(e => e.LastLoginAt)
            .IsRequired(false);


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

        builder.OwnsMany<UserRoleAssignment>(u => u.RoleAssignments, ra =>
        {
            ra.ToTable("UserRolesAssignments");
            ra.HasKey(r => new { r.UserId, r.RoleId });

            ra.WithOwner().HasForeignKey(r => r.UserId);

            ra.Property(e => e.RoleId)
              .IsRequired();

            ra.Property(e => e.AssignedAt)
                //.HasColumnType("DATETIMEOFFSET")
                .IsRequired();

            ra.Property(e => e.ExpiresAt)
                //.HasColumnType("DATETIMEOFFSET")
                .IsRequired(false);


            ra.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            ra.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            ra.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            ra.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            ra.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            ra.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            ra.Property(a => a.IsActive).HasColumnName("IsActive");

            // Foreign key relationships for audit fields
            ra.HasOne(e => e.CreatedByUser)
              .WithMany()
              .HasForeignKey(e => e.CreatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            ra.HasOne(e => e.UpdatedByUser)
              .WithMany()
              .HasForeignKey(e => e.UpdatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            ra.HasOne(e => e.DeletedByUser)
              .WithMany()
              .HasForeignKey(e => e.DeletedBy)
              .OnDelete(DeleteBehavior.NoAction);

        });
    }
}
