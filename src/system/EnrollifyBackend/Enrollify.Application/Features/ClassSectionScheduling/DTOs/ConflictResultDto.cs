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
  public string? DayOfWeek { get; init; }

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
