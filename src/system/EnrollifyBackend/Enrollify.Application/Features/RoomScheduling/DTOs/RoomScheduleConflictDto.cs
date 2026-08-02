namespace Enrollify.Application.Features.RoomScheduling.DTOs;

/// <summary>
/// A single overlapping offering that a block conflicts with (room double-booking).
/// </summary>
public sealed class RoomScheduleConflictDto
{
  public int OfferingId { get; init; }
  public int SectionId { get; init; }
  public string SectionName { get; init; } = string.Empty;
  public string SubjectCode { get; init; } = string.Empty;
  public string? TeacherName { get; init; }
  public string StartTime { get; init; } = string.Empty;
  public string EndTime { get; init; } = string.Empty;
}
