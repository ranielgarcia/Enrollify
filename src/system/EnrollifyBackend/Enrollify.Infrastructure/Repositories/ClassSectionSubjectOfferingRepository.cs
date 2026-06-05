using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionSubjectOfferingRepository : IClassSectionSubjectOfferingRepository
{
    private readonly EnrollifyDbContext _dbContext;
    private readonly ILogger<ClassSectionSubjectOfferingRepository> _logger;

    public ClassSectionSubjectOfferingRepository(EnrollifyDbContext dbContext, ILogger<ClassSectionSubjectOfferingRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<ClassSectionSubjectOfferingId>> Create(ClassSectionSubjectOffering newClassSectionSubjectOffering, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.ClassSectionSubjectOfferings.AddAsync(newClassSectionSubjectOffering, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(newClassSectionSubjectOffering.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new class section subject offering {@ClassSectionSubjectOffering}", newClassSectionSubjectOffering);
            return Result.Error("Unable to create the new class section subject offering due to internal error");
        }
    }

    public async Task<Result> Delete(ClassSectionSubjectOfferingId classSectionSubjectOfferingId, CancellationToken cancellationToken)
    {
        try
        {
            var subjectOffering = await _dbContext.ClassSectionSubjectOfferings.FirstOrDefaultAsync(o => o.Id == classSectionSubjectOfferingId, cancellationToken);
            if (subjectOffering == null)
            {
                return Result.NotFound($"Class section subject offering with an ID of {classSectionSubjectOfferingId.Value.ToString()} was not found.");
            }

            _dbContext.ClassSectionSubjectOfferings.Remove(subjectOffering);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class section subject offering {@ClassSectionSubjectOfferingId}", classSectionSubjectOfferingId);
            return Result.Error("Unable to delete class section subject offering due to internal error");
        }
    }

    public async Task<Result<ClassSectionSubjectOfferingId>> Update(ClassSectionSubjectOffering updatedClassSectionSubjectOffering, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.ClassSectionSubjectOfferings.Update(updatedClassSectionSubjectOffering);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(updatedClassSectionSubjectOffering.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class section subject offering {@ClassSectionSubjectOffering}", updatedClassSectionSubjectOffering);
            return Result.Error("Unable to update the class section subject offering due to internal error");
        }
    }
    
    // ==================== Conflict Detection Methods (Phase 1) ====================
    
    /// <summary>
    /// Checks if a teacher has a conflicting schedule at the given day/time in the same academic term.
    /// 
    /// IMPORTANT: Term scoping assumption
    /// This query assumes academic terms never have overlapping date ranges.
    /// Cross-term conflicts are not detected (e.g., if Fall and Intersession overlap).
    /// See: research/scheduling/class-scheduling-conflicts.md § IN-02
    /// If terms overlap in the future, add calendar date range checks.
    /// 
    /// Uses half-open interval overlap formula:
    /// A overlaps B iff A.StartTime &lt; B.EndTime AND A.EndTime &gt; B.StartTime
    /// </summary>
    public async Task<bool> HasTeacherScheduleConflictAsync(
        TeacherId teacherId,
        AcademicTermId academicTermId,
        DayOfWeekEnum dayOfWeek,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        ClassSectionSubjectOfferingId? excludeOfferingId,
        CancellationToken ct)
    {
        var teacherIdValue = (int)teacherId;
        var termIdValue = (int)academicTermId;
        var dayStr = dayOfWeek.Value; // "MON", "TUE", etc.
        int? excludeId = excludeOfferingId.HasValue ? (int)excludeOfferingId.Value : null;
        
        // Query: JOIN ClassSchedules → ClassSectionSubjectOffering → ClassSections
        // Filter by: TeacherId, AcademicTermId, DayOfWeek, time overlap
        // SNAPSHOT isolation level is already enabled database-wide
        return await _dbContext.Set<ClassSchedule>()
            .Where(cs => cs.IsActive && cs.DayOfWeek == dayStr)
            .Join(
                _dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive),
                cs => cs.ClassSectionSubjectOfferingId,
                o => o.Id,
                (cs, o) => new { cs, o })
            .Join(
                _dbContext.ClassSections.Where(s => s.IsActive),
                x => x.o.ClassSectionId,
                s => s.Id,
                (x, s) => new { x.cs, x.o, s })
            .AnyAsync(x =>
                (int?)x.o.TeacherId == teacherIdValue
                && (int)x.s.AcademicTermId == termIdValue
                && x.cs.StartTime < newEndTime
                && x.cs.EndTime > newStartTime
                && (excludeId == null || (int)x.o.Id != excludeId),
            ct);
    }
    
    /// <summary>
    /// Checks if a room has a conflicting schedule at the given day/time in the same academic term.
    /// See HasTeacherScheduleConflictAsync for implementation notes.
    /// </summary>
    public async Task<bool> HasRoomScheduleConflictAsync(
        RoomId roomId,
        AcademicTermId academicTermId,
        DayOfWeekEnum dayOfWeek,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        ClassSectionSubjectOfferingId? excludeOfferingId,
        CancellationToken ct)
    {
        var roomIdValue = (int)roomId;
        var termIdValue = (int)academicTermId;
        var dayStr = dayOfWeek.Value;
        int? excludeId = excludeOfferingId.HasValue ? (int)excludeOfferingId.Value : null;
        
        return await _dbContext.Set<ClassSchedule>()
            .Where(cs => cs.IsActive && cs.DayOfWeek == dayStr)
            .Join(
                _dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive),
                cs => cs.ClassSectionSubjectOfferingId,
                o => o.Id,
                (cs, o) => new { cs, o })
            .Join(
                _dbContext.ClassSections.Where(s => s.IsActive),
                x => x.o.ClassSectionId,
                s => s.Id,
                (x, s) => new { x.cs, x.o, s })
            .AnyAsync(x =>
                (int?)x.o.RoomId == roomIdValue
                && (int)x.s.AcademicTermId == termIdValue
                && x.cs.StartTime < newEndTime
                && x.cs.EndTime > newStartTime
                && (excludeId == null || (int)x.o.Id != excludeId),
            ct);
    }
    
    /// <summary>
    /// Checks if a section has overlapping schedules at the given day/time.
    /// Simpler than teacher/room checks since we already have the section ID.
    /// </summary>
    public async Task<bool> HasSectionScheduleOverlapAsync(
        ClassSectionId sectionId,
        DayOfWeekEnum dayOfWeek,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        ClassScheduleId? excludeScheduleId,
        CancellationToken ct)
    {
        var sectionIdValue = (int)sectionId;
        var dayStr = dayOfWeek.Value;
        int? excludeId = excludeScheduleId.HasValue ? (int)excludeScheduleId.Value : null;
        
        // Simpler query: no need to join ClassSections since we already have the section ID
        return await _dbContext.Set<ClassSchedule>()
            .Where(cs => cs.IsActive && cs.DayOfWeek == dayStr)
            .Join(
                _dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive),
                cs => cs.ClassSectionSubjectOfferingId,
                o => o.Id,
                (cs, o) => new { cs, o })
            .AnyAsync(x =>
                (int)x.o.ClassSectionId == sectionIdValue
                && x.cs.StartTime < newEndTime
                && x.cs.EndTime > newStartTime
                && (excludeId == null || (int)x.cs.Id != excludeId),
            ct);
    }
    
    /// <summary>
    /// Loads all schedule DTOs for offerings that share the given teacher or room IDs
    /// within the same academic term. Used for section detail conflict display (Read Path).
    /// Excludes the target section to avoid loading duplicate data.
    /// </summary>
    public async Task<List<ScheduleConflictDto>> GetRelatedSchedulesForConflictDetectionAsync(
        IEnumerable<int> teacherIds,
        IEnumerable<int> roomIds,
        int academicTermId,
        int excludeSectionId,
        CancellationToken ct)
    {
        var teacherIdList = teacherIds.ToList();
        var roomIdList = roomIds.ToList();
        
        // Load all schedules for offerings that share teachers or rooms in the same term,
        // excluding the target section (to avoid duplicate data)
        var query = _dbContext.Set<ClassSchedule>()
            .Where(cs => cs.IsActive)
            .Join(
                _dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive),
                cs => cs.ClassSectionSubjectOfferingId,
                o => o.Id,
                (cs, o) => new { cs, o })
            .Join(
                _dbContext.ClassSections.Where(s => s.IsActive),
                x => x.o.ClassSectionId,
                s => s.Id,
                (x, s) => new { x.cs, x.o, s })
            .Where(x =>
                (int)x.s.AcademicTermId == academicTermId
                && (int)x.s.Id != excludeSectionId
                && (teacherIdList.Contains((int?)x.o.TeacherId ?? -1) || roomIdList.Contains((int?)x.o.RoomId ?? -1)))
            .Select(x => new ScheduleConflictDto
            {
                ScheduleId = (int)x.cs.Id,
                OfferingId = (int)x.o.Id,
                SectionId = (int)x.s.Id,
                SectionName = x.s.Name,
                AcademicTermId = (int)x.s.AcademicTermId,
                
                TeacherId = x.o.TeacherId != null ? (int)x.o.TeacherId.Value : null,
                TeacherFirstName = x.o.Teacher != null ? x.o.Teacher.FirstName : null,
                TeacherLastName = x.o.Teacher != null ? x.o.Teacher.LastName : null,
                
                RoomId = x.o.RoomId != null ? (int)x.o.RoomId.Value : null,
                RoomNumber = x.o.Room != null ? x.o.Room.RoomNumber : null,
                BuildingName = x.o.Room != null && x.o.Room.Building != null ? x.o.Room.Building.Name : null,
                
                SubjectId = (int)x.o.SubjectId,
                SubjectCode = x.o.SnapshotSubjectCode != null ? (string)x.o.SnapshotSubjectCode : "",
                SubjectTitle = x.o.SnapshotSubjectTitle ?? "",
                
                DayOfWeek = x.cs.DayOfWeek,
                StartTime = x.cs.StartTime,
                EndTime = x.cs.EndTime
            });
        
        return await query.ToListAsync(ct);
    }
    
    /// <summary>
    /// Projects all schedules for a given section into flat DTOs for conflict detection.
    /// </summary>
    public async Task<List<ScheduleConflictDto>> GetSectionSchedulesForConflictDetectionAsync(
        int sectionId,
        CancellationToken ct)
    {
        var query = _dbContext.Set<ClassSchedule>()
            .Where(cs => cs.IsActive)
            .Join(
                _dbContext.ClassSectionSubjectOfferings.Where(o => o.IsActive),
                cs => cs.ClassSectionSubjectOfferingId,
                o => o.Id,
                (cs, o) => new { cs, o })
            .Join(
                _dbContext.ClassSections.Where(s => s.IsActive && (int)s.Id == sectionId),
                x => x.o.ClassSectionId,
                s => s.Id,
                (x, s) => new { x.cs, x.o, s })
            .Select(x => new ScheduleConflictDto
            {
                ScheduleId = (int)x.cs.Id,
                OfferingId = (int)x.o.Id,
                SectionId = (int)x.s.Id,
                SectionName = x.s.Name,
                AcademicTermId = (int)x.s.AcademicTermId,
                
                TeacherId = x.o.TeacherId != null ? (int)x.o.TeacherId.Value : null,
                TeacherFirstName = x.o.Teacher != null ? x.o.Teacher.FirstName : null,
                TeacherLastName = x.o.Teacher != null ? x.o.Teacher.LastName : null,
                
                RoomId = x.o.RoomId != null ? (int)x.o.RoomId.Value : null,
                RoomNumber = x.o.Room != null ? x.o.Room.RoomNumber : null,
                BuildingName = x.o.Room != null && x.o.Room.Building != null ? x.o.Room.Building.Name : null,
                
                SubjectId = (int)x.o.SubjectId,
                SubjectCode = x.o.SnapshotSubjectCode != null ? (string)x.o.SnapshotSubjectCode : "",
                SubjectTitle = x.o.SnapshotSubjectTitle ?? "",
                
                DayOfWeek = x.cs.DayOfWeek,
                StartTime = x.cs.StartTime,
                EndTime = x.cs.EndTime
            });
        
        return await query.ToListAsync(ct);
    }
}
