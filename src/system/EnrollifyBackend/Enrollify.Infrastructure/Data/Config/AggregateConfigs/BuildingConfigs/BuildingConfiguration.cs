using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.BuildingConfigs;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure (EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .IsRequired();

        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Description).IsRequired();

        // Audit fields
        builder.ConfigureAuditFields();
    }
}
