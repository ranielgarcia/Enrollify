namespace Enrollify.Application.Features.ClassSchedules.Models;

/// <summary>
/// Flat DTO projection for efficient conflict detection queries.
/// Represents a single schedule row with all context needed for conflict checks.
/// Used in both write-path validation and read-path conflict display.
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
    
    public string DayOfWeek { get; init; } = string.Empty; // "MON", "TUE", etc.
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}
