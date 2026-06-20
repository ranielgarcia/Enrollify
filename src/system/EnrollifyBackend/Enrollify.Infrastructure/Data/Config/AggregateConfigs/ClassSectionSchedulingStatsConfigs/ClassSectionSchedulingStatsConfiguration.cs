using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionSchedulingStatsConfigs;

public class ClassSectionSchedulingStatsConfiguration : IEntityTypeConfiguration<ClassSectionSchedulingStats>
{
  public void Configure(EntityTypeBuilder<ClassSectionSchedulingStats> builder)
  {
    builder.ToTable("ClassSectionSchedulingStats");

    builder.HasKey(e => e.Id);
    builder.Property(e => e.Id)
      .UseIdentityColumn()
      .IsRequired();

    builder.Property(e => e.CollegeId)
      .IsRequired();

    builder.Property(e => e.CourseId)
      .IsRequired();

    builder.Property(e => e.AcademicTermId)
      .IsRequired();

    builder.Property(e => e.ClassSectionId)
      .IsRequired(false);

    builder.Property(e => e.AggregateType)
      .HasColumnName("AggregateType")
      .HasConversion(v => v.Name
        , v => Core.Constants.ClassSectionSchedulingStatsAggregateTypeEnum.FromName(v))
      .IsRequired();

    builder.Property(e => e.AggregateCount)
      .IsRequired();

    builder.Property(e => e.ComputedAt).IsRequired();
  }
}
