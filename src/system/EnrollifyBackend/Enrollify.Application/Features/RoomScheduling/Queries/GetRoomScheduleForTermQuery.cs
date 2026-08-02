using System.Globalization;
using Enrollify.Application.Features.RoomScheduling.DTOs;
using Enrollify.Application.Features.RoomScheduling.Models;
using Enrollify.Application.Features.RoomScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Services.ScheduleConflictDetection;

namespace Enrollify.Application.Features.RoomScheduling.Queries;

/// <summary>
/// Builds the room-centric schedule grid for a single academic term and day of week.
/// Room double-booking conflicts (HC-02) are recomputed live via
/// <see cref="ClassScheduleConflictDetector" /> so the view is always accurate.
/// </summary>
public record GetRoomScheduleForTermQuery(
  int AcademicTermId,
  string DayOfWeek,
  int? BuildingId = null,
  int? RoomTypeId = null,
  int? CollegeId = null,
  int? CourseId = null) : IRequest<Result<RoomScheduleDto>>;

public class GetRoomScheduleForTermQueryHandler
  : IRequestHandler<GetRoomScheduleForTermQuery, Result<RoomScheduleDto>>
{
  private readonly IRoomScheduleReadRepository _readRepository;
  private readonly ClassScheduleConflictDetector _conflictDetector;

  public GetRoomScheduleForTermQueryHandler(
    IRoomScheduleReadRepository readRepository,
    ClassScheduleConflictDetector conflictDetector)
  {
    _readRepository = readRepository;
    _conflictDetector = conflictDetector;
  }

  public async Task<Result<RoomScheduleDto>> Handle(
    GetRoomScheduleForTermQuery request,
    CancellationToken cancellationToken)
  {
    if (!DayOfWeekEnum.TryFromValue(request.DayOfWeek, out DayOfWeekEnum day))
      return Result.Invalid(new ValidationError
      {
        Identifier = nameof(request.DayOfWeek),
        ErrorMessage = $"'{request.DayOfWeek}' is not a valid day of week. Expected one of: MON, TUE, WED, THU, FRI, SAT, SUN."
      });

    var filter = new RoomScheduleQueryFilter(
      request.AcademicTermId,
      day.Value,
      request.BuildingId,
      request.RoomTypeId,
      request.CollegeId,
      request.CourseId);

    RoomScheduleQueryResult data = await _readRepository.GetRoomScheduleDataAsync(filter, cancellationToken);

    // One meeting row per offering for the selected day (UNIQUE(OfferingId, DayOfWeek)).
    var offeringById = data.Offerings.ToDictionary(o => o.OfferingId);

    (Dictionary<int, HashSet<int>> adjacency, int conflictPairs) = DetectRoomConflicts(data.Offerings, day);

    var offeringDtoById = data.Offerings.ToDictionary(
      row => row.OfferingId,
      row => MapOffering(row, adjacency, offeringById));

    var assignedByRoom = data.Offerings
      .Where(o => o.RoomId.HasValue)
      .GroupBy(o => o.RoomId!.Value)
      .ToDictionary(g => g.Key, g => g.ToList());

    var roomDtos = data.Rooms
      .Select(room => new RoomScheduleRoomDto
      {
        Id = room.Id,
        RoomNumber = room.RoomNumber,
        Capacity = room.Capacity,
        RoomTypeName = room.RoomTypeName,
        BuildingId = room.BuildingId,
        BuildingName = room.BuildingName,
        CollegeId = room.CollegeId,
        CollegeCode = room.CollegeCode,
        Offerings = assignedByRoom.TryGetValue(room.Id, out List<RoomScheduleOfferingRow>? rows)
          ? rows.OrderBy(r => r.StartTime).Select(r => offeringDtoById[r.OfferingId]).ToList()
          : new List<RoomScheduleOfferingDto>()
      })
      .ToList();

    var unassigned = data.Offerings
      .Where(o => !o.RoomId.HasValue)
      .OrderBy(o => o.StartTime)
      .Select(o => offeringDtoById[o.OfferingId])
      .ToList();

    var dto = new RoomScheduleDto
    {
      AcademicTermId = request.AcademicTermId,
      DayOfWeek = day.Value,
      Rooms = roomDtos,
      UnassignedOfferings = unassigned,
      Stats = new RoomScheduleStatsDto
      {
        TotalRooms = data.Rooms.Count,
        RoomsInUse = assignedByRoom.Count,
        TotalOfferings = data.Offerings.Count,
        UnassignedOfferings = unassigned.Count,
        TotalConflicts = conflictPairs
      }
    };

    return Result.Success(dto);
  }

  /// <summary>
  /// Runs the shared conflict detector, keeps only room double-bookings, and expands each
  /// one-sided result into a symmetric adjacency so both blocks in a pair are flagged.
  /// </summary>
  private (Dictionary<int, HashSet<int>> Adjacency, int ConflictPairs) DetectRoomConflicts(
    List<RoomScheduleOfferingRow> offerings, DayOfWeekEnum day)
  {
    var projections = offerings.Select(o => new ClassScheduleConflictProjectionDto
    {
      ScheduleId = ClassScheduleId.From(o.ScheduleId),
      OfferingId = ClassSectionSubjectOfferingId.From(o.OfferingId),
      SectionId = ClassSectionId.From(o.SectionId),
      SectionName = o.SectionName,
      AcademicTermId = AcademicTermId.From(o.AcademicTermId),
      TeacherId = o.TeacherId.HasValue ? Core.Aggregates.TeacherAggregate.TeacherId.From(o.TeacherId.Value) : null,
      TeacherFirstName = o.TeacherFirstName,
      TeacherLastName = o.TeacherLastName,
      RoomId = o.RoomId.HasValue ? Core.Aggregates.RoomAggregate.RoomId.From(o.RoomId.Value) : null,
      SubjectId = SubjectId.From(o.SubjectId),
      SubjectCode = SubjectCode.From(o.SubjectCode),
      SubjectTitle = o.SubjectTitle,
      DayOfWeek = day,
      StartTime = o.StartTime,
      EndTime = o.EndTime
    });

    List<ClassScheduleConflictResult> results = _conflictDetector.DetectConflicts(projections);

    var adjacency = new Dictionary<int, HashSet<int>>();
    var seenPairs = new HashSet<(int, int)>();

    foreach (ClassScheduleConflictResult result in results)
    {
      if (result.Type != ClassSectionValidationIssueTypeEnum.ROOM_DOUBLE_BOOKED || result.OfferingId is null)
        continue;

      int a = result.OfferingId.Value.Value;
      foreach (ClassScheduleConflictingOffering other in result.ConflictingOfferings ?? new())
      {
        int b = other.Id.Value;
        if (a == b) continue;

        AddEdge(adjacency, a, b);
        AddEdge(adjacency, b, a);
        seenPairs.Add(a < b ? (a, b) : (b, a));
      }
    }

    return (adjacency, seenPairs.Count);
  }

  private static void AddEdge(Dictionary<int, HashSet<int>> adjacency, int from, int to)
  {
    if (!adjacency.TryGetValue(from, out HashSet<int>? set))
    {
      set = new HashSet<int>();
      adjacency[from] = set;
    }

    set.Add(to);
  }

  private static RoomScheduleOfferingDto MapOffering(
    RoomScheduleOfferingRow row,
    Dictionary<int, HashSet<int>> adjacency,
    Dictionary<int, RoomScheduleOfferingRow> offeringById)
  {
    var conflicts = new List<RoomScheduleConflictDto>();
    if (adjacency.TryGetValue(row.OfferingId, out HashSet<int>? others))
      foreach (int otherId in others)
        if (offeringById.TryGetValue(otherId, out RoomScheduleOfferingRow? otherRow))
          conflicts.Add(new RoomScheduleConflictDto
          {
            OfferingId = otherRow.OfferingId,
            SectionId = otherRow.SectionId,
            SectionName = otherRow.SectionName,
            SubjectCode = otherRow.SubjectCode,
            TeacherName = FormatTeacher(otherRow),
            StartTime = FormatTime(otherRow.StartTime),
            EndTime = FormatTime(otherRow.EndTime)
          });

    return new RoomScheduleOfferingDto
    {
      OfferingId = row.OfferingId,
      ScheduleId = row.ScheduleId,
      SectionId = row.SectionId,
      SectionName = row.SectionName,
      CourseId = row.CourseId,
      CourseCode = row.CourseCode,
      SubjectId = row.SubjectId,
      SubjectCode = row.SubjectCode,
      SubjectTitle = row.SubjectTitle,
      TeacherId = row.TeacherId,
      TeacherName = FormatTeacher(row),
      RoomId = row.RoomId,
      DayOfWeek = row.DayOfWeek,
      StartTime = FormatTime(row.StartTime),
      EndTime = FormatTime(row.EndTime),
      Conflicts = conflicts
    };
  }

  private static string? FormatTeacher(RoomScheduleOfferingRow row)
  {
    if (!row.TeacherId.HasValue) return null;
    return $"{row.TeacherFirstName} {row.TeacherLastName}".Trim();
  }

  private static string FormatTime(TimeOnly time) => time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
}
