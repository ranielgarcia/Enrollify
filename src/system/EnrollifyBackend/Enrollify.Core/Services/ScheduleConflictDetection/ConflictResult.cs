namespace Enrollify.Core.Services.ScheduleConflictDetection;

/// <summary>
/// Domain model representing a detected scheduling conflict.
/// This is the internal domain representation; use ConflictResultDto for API responses.
/// </summary>
public record ConflictResult
{
    public ConflictType Type { get; init; }
    public ConflictSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Day { get; init; }
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

/// <summary>
/// Conflict types for domain-level conflict detection.
/// Maps to ConflictTypeEnum in the application layer.
/// </summary>
public enum ConflictType
{
    // Phase 1 conflicts
    TeacherDoubleBooked,
    RoomDoubleBooked,
    SectionOverlap,
    DuplicateSubjectInSection,
    
    // Phase 3 conflicts (future)
    ScheduleCountMismatch,
    HoursMismatch,
    NoSchedules
}

/// <summary>
/// Conflict severity levels
/// </summary>
public enum ConflictSeverity
{
    Info,
    Warning,
    Error
}
