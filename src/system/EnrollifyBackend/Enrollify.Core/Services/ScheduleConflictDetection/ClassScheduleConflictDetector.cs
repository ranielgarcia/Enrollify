using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Services.ScheduleConflictDetection;

/// <summary>
/// Domain service for detecting scheduling conflicts in memory.
/// Used for section detail page conflict display (Read Path).
///
/// IMPORTANT: Term scoping assumption
/// This detector assumes academic terms never have overlapping date ranges.
/// Cross-term conflicts are not detected (e.g., if Fall and Intersession overlap).
/// See: research/scheduling/class-scheduling-conflicts.md § IN-02
/// If terms overlap in the future, add calendar date range checks.
///
/// Conflict detection uses half-open interval overlap formula: https://chatgpt.com/c/6a2d4921-7c40-83ec-86cd-0f8058a958cf
/// A overlaps B iff A.StartTime &lt; B.EndTime AND A.EndTime &gt; B.StartTime
/// Adjacent classes (A ends 10:30, B starts 10:30) are NOT conflicts (gap = 0).
/// Use SC-03 (TEACHER_NO_BREAK) separately to enforce minimum break time.
/// </summary>
public class ClassScheduleConflictDetector
{
  /// <summary>
  /// Detects all conflicts for a given set of schedule DTOs.
  /// This method assumes the input data is already filtered to the relevant scope
  /// (e.g., same academic term, related teachers/rooms).
  /// </summary>
  /// <param name="schedules">Flat list of schedule DTOs (this section + related schedules)</param>
  /// <param name="targetSectionId">The section being analyzed (optional, for scoping section overlap checks)</param>
  /// <returns>List of detected conflicts</returns>
  public List<ClassScheduleConflictResult> DetectConflicts(
    IEnumerable<ClassScheduleConflictProjectionDto> schedules,
    ClassSectionId? targetSectionId = null)
  {
    var conflicts = new List<ClassScheduleConflictResult>();
    var scheduleList = schedules.ToList();

    // HC-01: Teacher double-booked
    conflicts.AddRange(DetectTeacherConflicts(scheduleList));

    // HC-02: Room double-booked
    conflicts.AddRange(DetectRoomConflicts(scheduleList));

    // HC-03: Section overlap (only if targetSectionId is provided)
    if (targetSectionId.HasValue) conflicts.AddRange(DetectSectionOverlaps(scheduleList, targetSectionId.Value));

    // DI-05: Duplicate subject in section (cross-offering check)
    conflicts.AddRange(DetectDuplicateSubjects(scheduleList));

    return conflicts;
  }

  private List<ClassScheduleConflictResult> DetectTeacherConflicts(List<ClassScheduleConflictProjectionDto> schedules)
  {
    var conflicts = new List<ClassScheduleConflictResult>();

    // Group by (TeacherId, DayOfWeek) to reduce comparison space
    // This transforms O(n²) over all schedules to O(n²) within small groups (typically 2-10 rows)
    IEnumerable<IGrouping<(TeacherId Value, DayOfWeekEnum DayOfWeek), ClassScheduleConflictProjectionDto>>
      byTeacherDay =
        schedules
          .Where(s => s.TeacherId != null)
          .GroupBy(s => (s.TeacherId!.Value, s.DayOfWeek));

    foreach (IGrouping<(TeacherId Value, DayOfWeekEnum DayOfWeek), ClassScheduleConflictProjectionDto> group in
             byTeacherDay)
    {
      var groupList = group.ToList();

      // O(n²) within group (typically 2-10 rows, so ~10-100 comparisons)
      for (int i = 0; i < groupList.Count; i++)
      for (int j = i + 1; j < groupList.Count; j++)
      {
        ClassScheduleConflictProjectionDto s1 = groupList[i];
        ClassScheduleConflictProjectionDto s2 = groupList[j];

        // Skip if same offering (shouldn't happen due to duplicate-day constraint)
        if (s1.OfferingId == s2.OfferingId) continue;

        // Half-open interval overlap: A.start < B.end AND A.end > B.start
        if (s1.StartTime < s2.EndTime && s1.EndTime > s2.StartTime)
          conflicts.Add(new ClassScheduleConflictResult
          {
            OfferingId = s1.OfferingId, // Primary offering for linking to DTOs (arbitrary choice of s1 vs s2)
            Type = ClassSectionValidationIssueTypeEnum.TEACHER_DOUBLE_BOOKED,
            Severity = DomainValidationErrorSeverityEnum.Error,
            Message =
              $"{s1.TeacherFirstName} {s1.TeacherLastName} is assigned to multiple classes at {s1.DayOfWeek} {FormatTime(s1.StartTime)}-{FormatTime(s1.EndTime)}",
            DayOfWeek = s1.DayOfWeek,
            StartTime = s1.StartTime,
            EndTime = s1.EndTime,
            ConflictingOfferings = new List<ClassScheduleConflictingOffering>
            {
              MapToAffectedOffering(s2)
            }
          });
      }
    }

    return conflicts;
  }

  private List<ClassScheduleConflictResult> DetectRoomConflicts(List<ClassScheduleConflictProjectionDto> schedules)
  {
    var conflicts = new List<ClassScheduleConflictResult>();

    IEnumerable<IGrouping<(RoomId RoomId, DayOfWeekEnum DayOfWeek), ClassScheduleConflictProjectionDto>> byRoomDay =
      schedules
        .Where(s => s.RoomId != null)
        .GroupBy(s => (s.RoomId!.Value, s.DayOfWeek));

    foreach (IGrouping<(RoomId RoomId, DayOfWeekEnum DayOfWeek), ClassScheduleConflictProjectionDto> group in byRoomDay)
    {
      var groupList = group.ToList();

      for (int i = 0; i < groupList.Count; i++)
      for (int j = i + 1; j < groupList.Count; j++)
      {
        ClassScheduleConflictProjectionDto s1 = groupList[i];
        ClassScheduleConflictProjectionDto s2 = groupList[j];

        if (s1.OfferingId == s2.OfferingId) continue;

        if (s1.StartTime < s2.EndTime && s1.EndTime > s2.StartTime)
          conflicts.Add(new ClassScheduleConflictResult
          {
            OfferingId = s1.OfferingId,
            Type = ClassSectionValidationIssueTypeEnum.ROOM_DOUBLE_BOOKED,
            Severity = DomainValidationErrorSeverityEnum.Error,
            Message =
              $"Room {s1.RoomNumber} ({s1.BuildingName}) is assigned to multiple classes at {s1.DayOfWeek} {FormatTime(s1.StartTime)}-{FormatTime(s1.EndTime)}",
            DayOfWeek = s1.DayOfWeek,
            StartTime = s1.StartTime,
            EndTime = s1.EndTime,
            ConflictingOfferings = new List<ClassScheduleConflictingOffering>
            {
              MapToAffectedOffering(s2)
            }
          });
      }
    }

    return conflicts;
  }

  private List<ClassScheduleConflictResult> DetectSectionOverlaps(List<ClassScheduleConflictProjectionDto> schedules,
    ClassSectionId targetSectionId)
  {
    var conflicts = new List<ClassScheduleConflictResult>();

    // Only check schedules within the target section
    var sectionSchedules = schedules
      .Where(s => s.SectionId == targetSectionId)
      .ToList();

    IEnumerable<IGrouping<DayOfWeekEnum, ClassScheduleConflictProjectionDto>> bySectionDay = sectionSchedules
      .GroupBy(s => s.DayOfWeek);

    foreach (IGrouping<DayOfWeekEnum, ClassScheduleConflictProjectionDto> group in bySectionDay)
    {
      var groupList = group.ToList();

      for (int i = 0; i < groupList.Count; i++)
      for (int j = i + 1; j < groupList.Count; j++)
      {
        ClassScheduleConflictProjectionDto s1 = groupList[i];
        ClassScheduleConflictProjectionDto s2 = groupList[j];

        if (s1.OfferingId == s2.OfferingId) continue;

        if (s1.StartTime < s2.EndTime && s1.EndTime > s2.StartTime)
          conflicts.Add(new ClassScheduleConflictResult
          {
            OfferingId = s1.OfferingId,
            Type = ClassSectionValidationIssueTypeEnum.SECTION_OVERLAP,
            Severity = DomainValidationErrorSeverityEnum.Warning, // Soft conflict per design decision
            Message =
              $"Section {s1.SectionName} has overlapping classes at {s1.DayOfWeek} {FormatTime(s1.StartTime)}-{FormatTime(s1.EndTime)}",
            DayOfWeek = s1.DayOfWeek,
            StartTime = s1.StartTime,
            EndTime = s1.EndTime,
            ConflictingOfferings = new List<ClassScheduleConflictingOffering>
            {
              MapToAffectedOffering(s2)
            }
          });
      }
    }

    return conflicts;
  }

  private List<ClassScheduleConflictResult> DetectDuplicateSubjects(List<ClassScheduleConflictProjectionDto> schedules)
  {
    var conflicts = new List<ClassScheduleConflictResult>();

    // Group by (SectionId, SubjectId) and count
    IEnumerable<IGrouping<(ClassSectionId SectionId, SubjectId SubjectId), ClassScheduleConflictProjectionDto>>
      duplicates =
        schedules
          .GroupBy(s => (s.SectionId, s.SubjectId))
          .Where(g => g.DistinctBy(x => x.OfferingId).Count() > 1);

    foreach (IGrouping<(ClassSectionId SectionId, SubjectId SubjectId), ClassScheduleConflictProjectionDto> group in
             duplicates)
    {
      var offerings = group.ToList();
      ClassScheduleConflictProjectionDto first = offerings.First();

      conflicts.Add(new ClassScheduleConflictResult
      {
        Type = ClassSectionValidationIssueTypeEnum.DUPLICATE_SUBJECT_IN_SECTION,
        Severity = DomainValidationErrorSeverityEnum.Error,
        Message = $"Subject {first.SubjectCode} is assigned to section {first.SectionName} multiple times",
        ConflictingOfferings = offerings.Select(MapToAffectedOffering).ToList()
      });
    }

    return conflicts;
  }

  private ClassScheduleConflictingOffering MapToAffectedOffering(ClassScheduleConflictProjectionDto dto)
  {
    return new ClassScheduleConflictingOffering
    {
      Id = dto.OfferingId,
      Subject = new SubjectSummary(dto.SubjectCode, dto.SubjectTitle),
      Section = new SectionSummary(dto.SectionId, dto.SectionName),
      Room = dto.RoomId.HasValue
        ? new RoomSummary(dto.RoomNumber!, dto.BuildingName!)
        : null
    };
  }

  private string FormatTime(TimeOnly time)
  {
    return time.ToString("HH:mm");
  }
}
