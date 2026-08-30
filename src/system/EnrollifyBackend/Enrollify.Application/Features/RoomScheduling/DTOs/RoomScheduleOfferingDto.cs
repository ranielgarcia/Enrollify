namespace Enrollify.Application.Features.RoomScheduling.DTOs;

/// <summary>
/// A single scheduled offering block on the room-schedule grid for the selected day.
/// Times are formatted as HH:mm:ss to match the frontend schedule model.
/// </summary>
public sealed class RoomScheduleOfferingDto
{
  public int OfferingId { get; init; }
  public int ScheduleId { get; init; }
  public int SectionId { get; init; }
  public string SectionName { get; init; } = string.Empty;
  public int CourseId { get; init; }
  public string CourseCode { get; init; } = string.Empty;
  public int SubjectId { get; init; }
  public string SubjectCode { get; init; } = string.Empty;
  public string SubjectTitle { get; init; } = string.Empty;
  public int? TeacherId { get; init; }
  public string? TeacherName { get; init; }
  public int? RoomId { get; init; }
  public string DayOfWeek { get; init; } = string.Empty;
  public string StartTime { get; init; } = string.Empty;
  public string EndTime { get; init; } = string.Empty;

  /// <summary>Other offerings this block overlaps with in the same room. Empty when no conflict.</summary>
  public List<RoomScheduleConflictDto> Conflicts { get; init; } = new();

  public bool HasConflict => Conflicts.Count > 0;
}
