namespace Enrollify.Core.Services.ScheduleConflictDetection;

/// <summary>
/// Domain-level schedule DTO for conflict detection.
/// This is used by the ScheduleConflictDetector domain service.
/// The application layer has its own version for API projections.
/// </summary>
public record ScheduleConflictDto
{
    public int ScheduleId { get; init; }
    public int OfferingId { get; init; }
    public int SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public int AcademicTermId { get; init; }
    
    public int? TeacherId { get; init; }
    public string? TeacherFirstName { get; init; }
    public string? TeacherLastName { get; init; }
    
    public int? RoomId { get; init; }
    public string? RoomNumber { get; init; }
    public string? BuildingName { get; init; }
    
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectTitle { get; init; } = string.Empty;
    
    public string DayOfWeek { get; init; } = string.Empty;
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}
