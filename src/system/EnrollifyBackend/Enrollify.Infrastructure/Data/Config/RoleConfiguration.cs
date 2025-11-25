using Enrollify.Core.RoleAggregate;

namespace Enrollify.Infrastructure.Data.Config;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(RoleName.MaxLength)
            .HasColumnType($"VARCHAR({RoleName.MaxLength})");

        builder.Property(e => e.Description)
            .HasMaxLength(RoleDescription.MaxLength)
            .HasColumnType($"VARCHAR({RoleDescription.MaxLength})");

        // Audit fields
        builder.ConfigureAuditFields();

    }
}
