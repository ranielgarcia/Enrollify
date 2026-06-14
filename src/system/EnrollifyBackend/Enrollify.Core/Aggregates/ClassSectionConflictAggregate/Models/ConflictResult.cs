using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.ClassSectionConflictAggregate.Models;

public record ConflictResult
{
  public ClassScheduleConflictTypeEnum Type { get; init; }
  public DomainValidationErrorSeverityEnum Severity { get; init; }
  public string Message { get; init; } = string.Empty;
  public DayOfWeekEnum? DayOfWeek { get; init; }
  public TimeOnly? StartTime { get; init; }
  public TimeOnly? EndTime { get; init; }
  public List<AffectedOffering>? AffectedOfferings { get; init; }
}
