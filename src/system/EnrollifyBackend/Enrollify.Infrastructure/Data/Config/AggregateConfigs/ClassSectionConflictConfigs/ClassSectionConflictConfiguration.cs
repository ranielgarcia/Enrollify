using Enrollify.Core.Aggregates.ClassSectionConflictAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConflictConfigs;

public class ClassSectionConflictConfiguration : IEntityTypeConfiguration<ClassSectionConflict>
{
  public void Configure(EntityTypeBuilder<ClassSectionConflict> builder)
  {
    builder.ToTable("ClassSectionConflicts");

    builder.HasKey(e => e.Id);
    builder.Property(e => e.Id)
        .UseIdentityColumn()
        .IsRequired();

    builder.Property(e => e.CollegeId).IsRequired();
    builder.Property(e => e.CourseId).IsRequired();
    builder.Property(e => e.AcademicTermId).IsRequired();
    builder.Property(e => e.ClassSectionId).IsRequired();
    builder.Property(e => e.OfferingId).IsRequired();

    builder.Property(e => e.ConflictType)
        .HasConversion(
            v => v.Name,
            v => Core.Constants.ClassScheduleConflictTypeEnum.FromName(v))
        .HasColumnType("varchar(50)")
        .IsRequired();

    builder.Property(e => e.Severity)
        .HasConversion(
            v => v.Value,
            v => Core.Constants.DomainValidationErrorSeverityEnum.FromValue(v))
        .HasColumnType("int")
        .IsRequired();

    builder.Property(e => e.Message)
        .HasColumnType("varchar(255)")
        .IsRequired();

    builder.Property(e => e.DayOfWeek)
        .HasConversion(
            v => v.Value,
            v => Core.Constants.DayOfWeekEnum.FromValue(v))
        .HasColumnType("char(3)")
        .IsRequired();

    builder.Property(e => e.StartTime).IsRequired(false);
    builder.Property(e => e.EndTime).IsRequired(false);
    builder.Property(e => e.ComputedAt).IsRequired();

    // EF Core can't add/remove items via a read-only collection,
    // so you tell EF to use the backing field instead of the property
    // This is mainly about materialization and change-tracking without requiring a public setter or a mutable collection property.
    builder.Navigation(c => c.AffectedOfferings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.OwnsMany<ClassSectionConflictAffectedOffering>(c => c.AffectedOfferings, ao =>
    {
      ao.ToTable("ClassSectionConflictAffectedOfferings");

      ao.WithOwner().HasForeignKey(e => e.ConflictId);

      ao.HasKey(e => e.Id);
      ao.Property(e => e.Id)
              .UseIdentityColumn()
              .IsRequired();

      ao.Property(e => e.OfferingId).IsRequired();

    });
  }
}
