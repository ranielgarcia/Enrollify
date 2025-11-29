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

        // Audit fields
        builder.ConfigureAuditFields();

        builder.OwnsMany<UserRoleAssignment>(u => u.RoleAssignments, ra =>
        {
            ra.ToTable("UserRolesAssignments");
            ra.HasKey(r => r.Id);
            ra.Property(e => e.Id)
              .UseIdentityColumn()
              .IsRequired();

            ra.WithOwner().HasForeignKey(r => r.UserId);

            ra.Property(e => e.RoleId)
              .IsRequired();

            ra.Property(e => e.AssignedAt)
                //.HasColumnType("DATETIMEOFFSET")
                .IsRequired();

            ra.Property(e => e.ExpiresAt)
                //.HasColumnType("DATETIMEOFFSET")
                .IsRequired();

            ra.ConfigureAuditFields();
        });
    }
}
