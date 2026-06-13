using Dapper;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionSubjectOfferingScheduleConflictRepository : IClassSectionSubjectOfferingScheduleConflictRepository
{
  private readonly IDbConnectionFactory _connectionFactory;

  public ClassSectionSubjectOfferingScheduleConflictRepository(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
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
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = @"
            SELECT TOP 1 1
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            INNER JOIN ClassSections sec ON sec.Id = o.ClassSectionId
            WHERE o.TeacherId = @TeacherId
              AND sec.AcademicTermId = @AcademicTermId
              AND cs.DayOfWeek = @DayOfWeek
              AND cs.StartTime < @NewEndTime
              AND cs.EndTime > @NewStartTime
              AND o.IsActive = 1
              AND cs.IsActive = 1
              AND sec.IsActive = 1
              AND (@ExcludeOfferingId IS NULL OR o.Id != @ExcludeOfferingId)";

    int? exists = await conn.QueryFirstOrDefaultAsync<int?>(
      sql,
      new
      {
        TeacherId = (int)teacherId,
        AcademicTermId = (int)academicTermId,
        DayOfWeek = dayOfWeek.Value,
        NewStartTime = newStartTime,
        NewEndTime = newEndTime,
        ExcludeOfferingId = excludeOfferingId.HasValue ? (int?)excludeOfferingId.Value.Value : null
      });

    return exists.HasValue;
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
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = @"
            SELECT TOP 1 1
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            INNER JOIN ClassSections sec ON sec.Id = o.ClassSectionId
            WHERE o.RoomId = @RoomId
              AND sec.AcademicTermId = @AcademicTermId
              AND cs.DayOfWeek = @DayOfWeek
              AND cs.StartTime < @NewEndTime
              AND cs.EndTime > @NewStartTime
              AND o.IsActive = 1
              AND cs.IsActive = 1
              AND sec.IsActive = 1
              AND (@ExcludeOfferingId IS NULL OR o.Id != @ExcludeOfferingId)";

    int? exists = await conn.QueryFirstOrDefaultAsync<int?>(
      sql,
      new
      {
        RoomId = (int)roomId,
        AcademicTermId = (int)academicTermId,
        DayOfWeek = dayOfWeek.Value,
        NewStartTime = newStartTime,
        NewEndTime = newEndTime,
        ExcludeOfferingId = excludeOfferingId.HasValue ? (int?)excludeOfferingId.Value.Value : null
      });

    return exists.HasValue;
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
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = @"
            SELECT TOP 1 1
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            WHERE o.ClassSectionId = @SectionId
              AND cs.DayOfWeek = @DayOfWeek
              AND cs.StartTime < @NewEndTime
              AND cs.EndTime > @NewStartTime
              AND o.IsActive = 1
              AND cs.IsActive = 1
              AND (@ExcludeScheduleId IS NULL OR cs.Id != @ExcludeScheduleId)";

    int? exists = await conn.QueryFirstOrDefaultAsync<int?>(
      sql,
      new
      {
        SectionId = (int)sectionId,
        DayOfWeek = dayOfWeek.Value,
        NewStartTime = newStartTime,
        NewEndTime = newEndTime,
        ExcludeScheduleId = excludeScheduleId.HasValue ? (int?)excludeScheduleId.Value.Value : null
      });

    return exists.HasValue;
  }

  /// <summary>
  /// Loads all schedule DTOs for offerings that share the given teacher or room IDs
  /// within the same academic term. Used for section detail conflict display (Read Path).
  /// Excludes the target section to avoid loading duplicate data.
  /// </summary>
  public async Task<List<ScheduleConflictProjectionDto>> GetRelatedSchedulesForConflictDetectionAsync(
    IEnumerable<int> teacherIds,
    IEnumerable<int> roomIds,
    int academicTermId,
    int excludeSectionId,
    CancellationToken ct)
  {
    var teacherIdList = teacherIds.ToList();
    var roomIdList = roomIds.ToList();

    // If no teachers or rooms to check, return empty list
    if (!teacherIdList.Any() && !roomIdList.Any()) return new List<ScheduleConflictProjectionDto>();

    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = @"
            SELECT
                cs.Id AS ScheduleId,
                o.Id AS OfferingId,
                sec.Id AS SectionId,
                CONCAT(sec.Name,'-',sec.IntendedYearLevel, sec.SectionCode) AS SectionName,
                sec.AcademicTermId,

                o.TeacherId,
                t.FirstName AS TeacherFirstName,
                t.LastName AS TeacherLastName,

                o.RoomId,
                r.RoomNumber,
                b.Name AS BuildingName,

                o.SubjectId,
                o.SnapshotSubjectCode AS SubjectCode,
                o.SnapshotSubjectTitle AS SubjectTitle,

                cs.DayOfWeek,
                cs.StartTime,
                cs.EndTime
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            INNER JOIN ClassSections sec ON sec.Id = o.ClassSectionId
            LEFT JOIN Teachers t ON t.Id = o.TeacherId
            LEFT JOIN Rooms r ON r.Id = o.RoomId
            LEFT JOIN Buildings b ON b.Id = r.BuildingId
            WHERE cs.IsActive = 1
              AND o.IsActive = 1
              AND sec.IsActive = 1
              AND sec.AcademicTermId = @AcademicTermId
              AND sec.Id != @ExcludeSectionId
              AND (
                  (@HasTeachers = 1 AND o.TeacherId IN @TeacherIds)
                  OR (@HasRooms = 1 AND o.RoomId IN @RoomIds)
              )";

    IEnumerable<ScheduleConflictProjectionDto> results = await conn.QueryAsync<ScheduleConflictProjectionDto>(
      sql,
      new
      {
        AcademicTermId = academicTermId,
        ExcludeSectionId = excludeSectionId,
        HasTeachers = teacherIdList.Any() ? 1 : 0,
        TeacherIds = teacherIdList.Any() ? teacherIdList : new List<int> { -1 },
        HasRooms = roomIdList.Any() ? 1 : 0,
        RoomIds = roomIdList.Any() ? roomIdList : new List<int> { -1 }
      });

    return results.ToList();
  }

  /// <summary>
  /// Projects all schedules for a given section into flat DTOs for conflict detection.
  /// </summary>
  public async Task<List<ScheduleConflictProjectionDto>> GetSectionSchedulesForConflictDetectionAsync(
    int sectionId,
    CancellationToken ct)
  {
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

    string sql = @"
            SELECT
                cs.Id AS ScheduleId,
                o.Id AS OfferingId,
                sec.Id AS SectionId,
                CONCAT(sec.Name,'-',sec.IntendedYearLevel, sec.SectionCode) AS SectionName,
                sec.AcademicTermId,

                o.TeacherId,
                t.FirstName AS TeacherFirstName,
                t.LastName AS TeacherLastName,

                o.RoomId,
                r.RoomNumber,
                b.Name AS BuildingName,

                o.SubjectId,
                o.SnapshotSubjectCode AS SubjectCode,
                o.SnapshotSubjectTitle AS SubjectTitle,

                cs.DayOfWeek,
                cs.StartTime,
                cs.EndTime
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            INNER JOIN ClassSections sec ON sec.Id = o.ClassSectionId
            LEFT JOIN Teachers t ON t.Id = o.TeacherId
            LEFT JOIN Rooms r ON r.Id = o.RoomId
            LEFT JOIN Buildings b ON b.Id = r.BuildingId
            WHERE cs.IsActive = 1
              AND o.IsActive = 1
              AND sec.IsActive = 1
              AND sec.Id = @SectionId";

    IEnumerable<ScheduleConflictProjectionDto> results = await conn.QueryAsync<ScheduleConflictProjectionDto>(
      sql,
      new { SectionId = sectionId });

    return results.ToList();
  }

  public async Task<List<ScheduleConflictProjectionDto>> GetOfferingSchedulesForConflictDetectionAsync(int offeringId, CancellationToken cancellationToken)
  {
    using SqlConnection conn = await _connectionFactory.CreateOpenAsync(cancellationToken);

    string sql = @"
            SELECT
                cs.Id AS ScheduleId,
                o.Id AS OfferingId,
                sec.Id AS SectionId,
                CONCAT(sec.Name,'-',sec.IntendedYearLevel, sec.SectionCode) AS SectionName,
                sec.AcademicTermId,

                o.TeacherId,
                t.FirstName AS TeacherFirstName,
                t.LastName AS TeacherLastName,

                o.RoomId,
                r.RoomNumber,
                b.Name AS BuildingName,

                o.SubjectId,
                o.SnapshotSubjectCode AS SubjectCode,
                o.SnapshotSubjectTitle AS SubjectTitle,

                cs.DayOfWeek,
                cs.StartTime,
                cs.EndTime
            FROM ClassSchedules cs
            INNER JOIN ClassSectionSubjectOffering o ON o.Id = cs.ClassSectionSubjectOfferingId
            INNER JOIN ClassSections sec ON sec.Id = o.ClassSectionId
            LEFT JOIN Teachers t ON t.Id = o.TeacherId
            LEFT JOIN Rooms r ON r.Id = o.RoomId
            LEFT JOIN Buildings b ON b.Id = r.BuildingId
            WHERE cs.IsActive = 1
              AND o.IsActive = 1
              AND sec.IsActive = 1
              AND o.Id = @OfferingId";

    IEnumerable<ScheduleConflictProjectionDto> results = await conn.QueryAsync<ScheduleConflictProjectionDto>(
      sql,
      new { OfferingId = offeringId });

    return results.ToList();
  }
}
