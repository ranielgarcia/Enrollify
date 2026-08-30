using Enrollify.Application.Features.RoomScheduling.Models;

namespace Enrollify.Application.Features.RoomScheduling.Repositories;

/// <summary>
/// Filter criteria for the room-schedule read model. Term and day are required;
/// building / room type / college narrow the room set, course narrows the offerings.
/// </summary>
public sealed record RoomScheduleQueryFilter(
  int AcademicTermId,
  string DayOfWeek,
  int? BuildingId,
  int? RoomTypeId,
  int? CollegeId,
  int? CourseId);

/// <summary>
/// Result of the room-schedule read model: the filtered rooms and the offering
/// meetings scheduled on the requested day.
/// </summary>
public sealed record RoomScheduleQueryResult(
  List<RoomScheduleRoomRow> Rooms,
  List<RoomScheduleOfferingRow> Offerings);

/// <summary>
/// Read-only repository that aggregates rooms and scheduled offerings for the
/// room-centric schedule grid in a single round-trip.
/// </summary>
public interface IRoomScheduleReadRepository
{
  Task<RoomScheduleQueryResult> GetRoomScheduleDataAsync(
    RoomScheduleQueryFilter filter,
    CancellationToken cancellationToken);
}
