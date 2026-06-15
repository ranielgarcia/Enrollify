using System.Text.Json;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConflictConfigs;

public class ClassSectionValidationIssueConfiguration : IEntityTypeConfiguration<ClassSectionValidationIssue>
{
  public void Configure(EntityTypeBuilder<ClassSectionValidationIssue> builder)
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

    builder.Property(e => e.Type)
      .HasConversion(
        v => v.Name,
        v => Core.Constants.ClassSectionValidationIssueTypeEnum.FromName(v))
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

    builder.Property(e => e.ConflictingOfferings)
      .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<ClassScheduleConflictingOffering>>(v, (JsonSerializerOptions?)null) ??
             new List<ClassScheduleConflictingOffering>())
      .HasColumnName("ConflictingOfferingsJson")
      .HasColumnType("nvarchar(max)")
      .IsRequired(false);
  }
}
