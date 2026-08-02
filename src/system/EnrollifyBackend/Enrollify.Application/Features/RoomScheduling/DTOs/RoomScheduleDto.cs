namespace Enrollify.Application.Features.RoomScheduling.DTOs;

/// <summary>
/// Full room-schedule read model for a term and day: rooms with their offerings,
/// offerings with no assigned room, and summary statistics.
/// </summary>
public sealed class RoomScheduleDto
{
  public int AcademicTermId { get; init; }
  public string DayOfWeek { get; init; } = string.Empty;
  public List<RoomScheduleRoomDto> Rooms { get; init; } = new();
  public List<RoomScheduleOfferingDto> UnassignedOfferings { get; init; } = new();
  public RoomScheduleStatsDto Stats { get; init; } = new();
}
