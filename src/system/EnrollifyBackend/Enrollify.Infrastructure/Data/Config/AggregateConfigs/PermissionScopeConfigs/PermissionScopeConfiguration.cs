using Enrollify.Core.Aggregates.PermissionScopeAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.PermissionScopeConfigs;

public class PermissionScopeConfiguration : IEntityTypeConfiguration<PermissionScope>
{
    public void Configure(EntityTypeBuilder<PermissionScope> builder)
    {
        builder.ToTable("PermissionScopes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("Name");

        // Audit fields
        builder.ConfigureAuditFields();
    }
}
