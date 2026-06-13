using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.Services.ScheduleConflictDetection;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionSubjectOfferingRepository
{
  Task<Result<ClassSectionSubjectOfferingId>> Create(ClassSectionSubjectOffering newClassSectionSubjectOffering,
    CancellationToken cancellationToken);

  Task<Result<ClassSectionSubjectOfferingId>> Update(ClassSectionSubjectOffering updatedClassSectionSubjectOffering,
    CancellationToken cancellationToken);

  Task<Result> Delete(ClassSectionSubjectOffering classSectionSubjectOffering, CancellationToken cancellationToken);

  // Conflict detection methods (Phase 1)

  /// <summary>
  /// Checks if a teacher has a conflicting schedule at the given day/time in the same academic term.
  /// Used for write-time conflict detection (HC-01: Teacher Double-Booked).
  /// </summary>
  Task<bool> HasTeacherScheduleConflictAsync(
    TeacherId teacherId,
    AcademicTermId academicTermId,
    DayOfWeekEnum dayOfWeek,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    ClassSectionSubjectOfferingId? excludeOfferingId,
    CancellationToken cancellationToken);

  /// <summary>
  /// Checks if a room has a conflicting schedule at the given day/time in the same academic term.
  /// Used for write-time conflict detection (HC-02: Room Double-Booked).
  /// </summary>
  Task<bool> HasRoomScheduleConflictAsync(
    RoomId roomId,
    AcademicTermId academicTermId,
    DayOfWeekEnum dayOfWeek,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    ClassSectionSubjectOfferingId? excludeOfferingId,
    CancellationToken cancellationToken);

  /// <summary>
  /// Checks if a section has overlapping schedules at the given day/time.
  /// Used for write-time conflict detection (HC-03: Section Overlap).
  /// </summary>
  Task<bool> HasSectionScheduleOverlapAsync(
    ClassSectionId sectionId,
    DayOfWeekEnum dayOfWeek,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    ClassScheduleId? excludeScheduleId,
    CancellationToken cancellationToken);

  /// <summary>
  /// Loads all schedule DTOs for offerings that share the given teacher or room IDs
  /// within the same academic term. Used for section detail conflict display (Read Path).
  /// </summary>
  Task<List<ScheduleConflictDto>> GetRelatedSchedulesForConflictDetectionAsync(
    IEnumerable<int> teacherIds,
    IEnumerable<int> roomIds,
    int academicTermId,
    int excludeSectionId,
    CancellationToken cancellationToken);

  /// <summary>
  /// Projects all schedules for a given section into flat DTOs for conflict detection.
  /// </summary>
  Task<List<ScheduleConflictDto>> GetSectionSchedulesForConflictDetectionAsync(
    int sectionId,
    CancellationToken cancellationToken);
}
