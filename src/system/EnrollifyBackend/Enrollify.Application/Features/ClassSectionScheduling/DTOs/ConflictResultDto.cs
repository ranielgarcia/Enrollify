namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

/// <summary>
/// Represents a detected scheduling conflict.
/// Matches the frontend ConflictResult type in offering.ts.
/// Used in both API responses and section detail queries.
/// </summary>
public record ConflictResultDto
{
  /// <summary>
  /// Optional unique identifier for the conflict instance (for deduplication)
  /// </summary>
  public string? Id { get; init; }

  /// <summary>
  /// Conflict type code (HC-01, HC-02, etc.)
  /// </summary>
  public string Type { get; init; }

  /// <summary>
  /// Severity level: Error (blocks enrollment), Warning (advisory), Info
  /// </summary>
  public string Severity { get; init; }

  /// <summary>
  /// Human-readable conflict description
  /// </summary>
  public string Message { get; init; } = string.Empty;

  /// <summary>
  /// Day of week where conflict occurs (e.g., "MON")
  /// </summary>
  public string? Day { get; init; }

  /// <summary>
  /// Start time of conflict window (e.g., "09:00:00")
  /// </summary>
  public string? StartTime { get; init; }

  /// <summary>
  /// End time of conflict window (e.g., "10:30:00")
  /// </summary>
  public string? EndTime { get; init; }

  /// <summary>
  /// List of offerings affected by this conflict
  /// </summary>
  public List<AffectedOfferingDto>? AffectedOfferings { get; init; }
}

/// <summary>
/// Summary of an offering affected by a conflict
/// </summary>
public record AffectedOfferingDto
{
  public int Id { get; init; }
  public SubjectSummaryDto Subject { get; init; } = null!;
  public SectionSummaryDto Section { get; init; } = null!;
  public RoomSummaryDto? Room { get; init; }
}

public record SubjectSummaryDto(string Code, string Title);

public record SectionSummaryDto(int Id, string Name);

public record RoomSummaryDto(string RoomNumber, string Building);

/// <summary>
/// Conflict type enum matching the research document taxonomy.
/// Phase 1 implements: HC-01, HC-02, HC-03, DI-05
/// Phase 3 will add: SC-01 through SC-06, DI-01 through DI-04, IN-01, IN-02
/// </summary>
public enum ConflictTypeEnum
{
  // Hard Conflicts (Tier 1) - Phase 1
  TEACHER_DOUBLE_BOOKED,
  ROOM_DOUBLE_BOOKED,
  SECTION_OVERLAP,
  DUPLICATE_DAY_IN_OFFERING,

  // Soft Conflicts (Tier 2) - Phase 3
  TEACHER_OVERLOAD,
  ROOM_CAPACITY_EXCEEDED,
  TEACHER_NO_BREAK,
  ADVISER_AS_TEACHER,
  YEAR_LEVEL_MISMATCH,
  OUTSIDE_OPERATING_HOURS,

  // Data Integrity (Tier 3) - Phase 3
  SCHEDULE_COUNT_MISMATCH,
  HOURS_MISMATCH,
  NO_SCHEDULES,
  NO_OFFERINGS,
  DUPLICATE_SUBJECT_IN_SECTION,

  // Informational (Tier 4) - Phase 3
  ROOM_TYPE_MISMATCH,
  CROSS_TERM_BOOKING
}

/// <summary>
/// Conflict severity levels
/// </summary>
public enum ConflictSeverityEnum
{
  /// <summary>
  /// Informational only - no action required
  /// </summary>
  Info,

  /// <summary>
  /// Warning - should be reviewed but not blocking
  /// </summary>
  Warning,

  /// <summary>
  /// Error - blocks enrollment opening (but not schedule save)
  /// </summary>
  Error
}
