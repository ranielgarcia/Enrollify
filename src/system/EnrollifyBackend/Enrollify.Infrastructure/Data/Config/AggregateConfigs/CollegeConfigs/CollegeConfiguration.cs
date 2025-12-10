using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CollegeConfigs;

public class CollegeConfiguration : IEntityTypeConfiguration<College>
{
    public void Configure (EntityTypeBuilder<College> builder)
    {
        builder.ToTable("Colleges");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .IsRequired();

        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.ConfigureAuditFields();
    }
}
