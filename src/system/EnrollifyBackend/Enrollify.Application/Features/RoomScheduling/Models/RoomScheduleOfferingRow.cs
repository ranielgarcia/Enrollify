namespace Enrollify.Application.Features.RoomScheduling.Models;

/// <summary>
/// Flat Dapper projection for a single scheduled offering meeting (one row per
/// offering per day) in the room-schedule read model.
/// </summary>
public sealed class RoomScheduleOfferingRow
{
  public int ScheduleId { get; init; }
  public int OfferingId { get; init; }
  public int SectionId { get; init; }
  public string SectionName { get; init; } = string.Empty;
  public int CourseId { get; init; }
  public string CourseCode { get; init; } = string.Empty;
  public int AcademicTermId { get; init; }

  public int? RoomId { get; init; }

  public int? TeacherId { get; init; }
  public string? TeacherFirstName { get; init; }
  public string? TeacherLastName { get; init; }

  public int SubjectId { get; init; }
  public string SubjectCode { get; init; } = string.Empty;
  public string SubjectTitle { get; init; } = string.Empty;

  public string DayOfWeek { get; init; } = string.Empty;
  public TimeOnly StartTime { get; init; }
  public TimeOnly EndTime { get; init; }
}
