using Enrollify.Core.Constants;

namespace Enrollify.Core.Services.ScheduleConflictDetection;

/// <summary>
/// Domain model representing a detected scheduling conflict.
/// This is the internal domain representation; use ConflictResultDto for API responses.
/// </summary>
public record ConflictResult
{
  public ClassScheduleConflictTypeEnum Type { get; init; }
  public DomainValidationErrorSeverityEnum Severity { get; init; }
  public string Message { get; init; } = string.Empty;
  public string? DayOfWeek { get; init; }
  public TimeOnly? StartTime { get; init; }
  public TimeOnly? EndTime { get; init; }
  public List<AffectedOffering>? AffectedOfferings { get; init; }
}

/// <summary>
/// Summary of an offering affected by a conflict
/// </summary>
public record AffectedOffering
{
  public int Id { get; init; }
  public SubjectSummary Subject { get; init; } = null!;
  public SectionSummary Section { get; init; } = null!;
  public RoomSummary? Room { get; init; }
}

public record SubjectSummary(string Code, string Title);
public record SectionSummary(int Id, string Name);
public record RoomSummary(string RoomNumber, string Building);

