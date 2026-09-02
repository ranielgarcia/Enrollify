using System.Text.Json;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Constants;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionValidationIssueConfigs;

public class ClassSectionValidationIssueConfiguration : IEntityTypeConfiguration<ClassSectionValidationIssue>
{
  public void Configure(EntityTypeBuilder<ClassSectionValidationIssue> builder)
  {
    builder.ToTable("ClassSectionValidationIssues");

    builder.HasKey(e => e.Id);
    builder.Property(e => e.Id)
      .UseIdentityColumn()
      .IsRequired();

    builder.Property(e => e.CourseId).IsRequired();
    builder.Property(e => e.AcademicTermId).IsRequired();
    builder.Property(e => e.ClassSectionId).IsRequired();
    builder.Property(e => e.OfferingId).IsRequired(false);

    builder.Property(e => e.Type)
      .HasConversion(
        v => v.Name,
        v => ClassSectionValidationIssueTypeEnum.FromName(v))
      .HasColumnType("varchar(50)")
      .IsRequired();

    builder.Property(e => e.Message)
      .HasColumnType("varchar(255)")
      .IsRequired();

    builder.Property(e => e.DayOfWeek)
      .HasConversion(
        v => SerializeDayOfWeek(v),
        v => DeserializeDayOfWeek(v))
      .HasColumnType("char(3)")
      .IsRequired(false);

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

  private static string? SerializeDayOfWeek(DayOfWeekEnum? dayOfWeek)
  {
    return dayOfWeek?.Value;
  }

  private static DayOfWeekEnum? DeserializeDayOfWeek(string? value)
  {
    if (value is null)
    {
      return null;
    }

    if (string.IsNullOrWhiteSpace(value))
    {
      throw new InvalidOperationException("Class section validation issue day-of-week metadata cannot be empty when a database value is present.");
    }

    return DayOfWeekEnum.FromValue(value);
  }
}
