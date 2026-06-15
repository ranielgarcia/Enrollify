using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;

public record ClassScheduleConflictResult
{
  public ClassSectionSubjectOfferingId? OfferingId { get; set; }
  public required ClassSectionValidationIssueTypeEnum Type { get; init; }
  public required DomainValidationErrorSeverityEnum Severity { get; init; }
  public required string Message { get; init; } = string.Empty;
  public DayOfWeekEnum? DayOfWeek { get; init; }
  public TimeOnly? StartTime { get; init; }
  public TimeOnly? EndTime { get; init; }
  public List<ClassScheduleConflictingOffering>? ConflictingOfferings { get; init; }
}
