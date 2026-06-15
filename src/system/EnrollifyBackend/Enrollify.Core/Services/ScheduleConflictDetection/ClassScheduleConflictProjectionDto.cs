using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Services.ScheduleConflictDetection;

/// <summary>
/// Domain-level schedule DTO for conflict detection.
/// This is used by the ClassScheduleConflictDetector domain service.
/// The application layer has its own version for API projections.
/// </summary>
public record ClassScheduleConflictProjectionDto
{
  public ClassScheduleId ScheduleId { get; init; }
  public ClassSectionSubjectOfferingId OfferingId { get; init; }
  public ClassSectionId SectionId { get; init; }
  public string SectionName { get; init; } = string.Empty;
  public AcademicTermId AcademicTermId { get; init; }

  public TeacherId? TeacherId { get; init; }
  public string? TeacherFirstName { get; init; }
  public string? TeacherLastName { get; init; }

  public RoomId? RoomId { get; init; }
  public string? RoomNumber { get; init; }
  public string? BuildingName { get; init; }

  public SubjectId SubjectId { get; init; }
  public SubjectCode SubjectCode { get; init; }
  public string SubjectTitle { get; init; } = string.Empty;

  public DayOfWeekEnum DayOfWeek { get; init; } = null!;
  public TimeOnly StartTime { get; init; }
  public TimeOnly EndTime { get; init; }
}
