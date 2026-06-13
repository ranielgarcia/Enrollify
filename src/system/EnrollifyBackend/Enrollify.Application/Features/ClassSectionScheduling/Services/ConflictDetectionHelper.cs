using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Services.ScheduleConflictDetection;

namespace Enrollify.Application.Features.ClassSectionScheduling.Services;

/// <summary>
/// Shared helper service for detecting schedule conflicts.
/// Used by section detail queries, offering queries, and schedule commands.
/// </summary>
public class ConflictDetectionHelper
{
  private readonly IClassSectionSubjectOfferingScheduleConflictRepository _conflictRepo;
  private readonly ScheduleConflictDetector _conflictDetector;

  public ConflictDetectionHelper(
    IClassSectionSubjectOfferingScheduleConflictRepository conflictRepo,
    ScheduleConflictDetector conflictDetector)
  {
    _conflictRepo = conflictRepo;
    _conflictDetector = conflictDetector;
  }

  /// <summary>
  /// Detects all conflicts for offerings in a given section.
  /// Returns conflicts grouped by offering ID.
  /// </summary>
  public async Task<Dictionary<int, List<ConflictResultDto>>> DetectConflictsForSectionAsync(
    ClassSection section,
    IEnumerable<int> teacherIds,
    IEnumerable<int> roomIds,
    CancellationToken cancellationToken)
  {
    var teacherIdList = teacherIds.ToList();
    var roomIdList = roomIds.ToList();

    // If no teachers or rooms assigned, only check section-level conflicts
    if (!teacherIdList.Any() && !roomIdList.Any())
    {
      // Still check for section overlap conflicts
      List<ScheduleConflictProjectionDto> sectionOnlySchedules =
        await _conflictRepo.GetSectionSchedulesForConflictDetectionAsync(
          section.Id.Value, cancellationToken);
      List<ConflictResult> sectionOnlyConflicts =
        _conflictDetector.DetectConflicts(sectionOnlySchedules, section.Id.Value);
      return GroupConflictsByOffering(sectionOnlyConflicts);
    }

    // Load this section's schedules
    List<ScheduleConflictProjectionDto> thisSectionSchedules =
      await _conflictRepo.GetSectionSchedulesForConflictDetectionAsync(
        section.Id.Value, cancellationToken);

    // Load related schedules for same teacher/room in same term (excluding this section)
    List<ScheduleConflictProjectionDto> relatedSchedules = await _conflictRepo.GetRelatedSchedulesForConflictDetectionAsync(
      teacherIdList, roomIdList, section.AcademicTermId.Value, section.Id.Value, cancellationToken);

    // Combine and detect conflicts
    var allSchedules = thisSectionSchedules.Concat(relatedSchedules).ToList();
    List<ConflictResult> domainConflicts = _conflictDetector.DetectConflicts(allSchedules, section.Id.Value);

    return GroupConflictsByOffering(domainConflicts);
  }

  /// <summary>
  /// Groups conflicts by offering ID for easy embedding in DTOs.
  /// </summary>
  private Dictionary<int, List<ConflictResultDto>> GroupConflictsByOffering(List<ConflictResult> conflicts)
  {
    var conflictsByOffering = conflicts
      .SelectMany(c => c.AffectedOfferings ?? new List<AffectedOffering>(),
        (conflict, affected) => new { conflict, affected })
      .GroupBy(x => x.affected.Id)
      .ToDictionary(g => g.Key, g => g.Select(x => MapConflictToDto(x.conflict)).Distinct().ToList());

    return conflictsByOffering;
  }

  /// <summary>
  /// Maps a domain ConflictResult to a DTO.
  /// </summary>
  private ConflictResultDto MapConflictToDto(ConflictResult conflict)
  {
    return new ConflictResultDto
    {
      Type = conflict.Type.Name,
      Severity = conflict.Severity.Name,
      Message = conflict.Message,
      DayOfWeek = conflict.DayOfWeek,
      StartTime = conflict.StartTime?.ToString("HH:mm:ss"),
      EndTime = conflict.EndTime?.ToString("HH:mm:ss"),
      AffectedOfferings = conflict.AffectedOfferings?.Select(a => new AffectedOfferingDto
      {
        Id = a.Id,
        Subject = new SubjectSummaryDto(a.Subject.Code, a.Subject.Title),
        Section = new SectionSummaryDto(a.Section.Id, a.Section.Name),
        Room = a.Room != null
          ? new RoomSummaryDto(a.Room.RoomNumber, a.Room.Building)
          : null
      }).ToList()
    };
  }
}
