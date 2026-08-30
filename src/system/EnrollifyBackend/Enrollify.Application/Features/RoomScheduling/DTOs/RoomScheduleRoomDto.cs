namespace Enrollify.Application.Features.RoomScheduling.DTOs;

/// <summary>
/// A single room row on the room-schedule grid, with its offerings for the selected day.
/// </summary>
public sealed class RoomScheduleRoomDto
{
  public int Id { get; init; }
  public string RoomNumber { get; init; } = string.Empty;
  public int Capacity { get; init; }
  public string? RoomTypeName { get; init; }
  public int? BuildingId { get; init; }
  public string? BuildingName { get; init; }
  public int? CollegeId { get; init; }
  public string? CollegeCode { get; init; }
  public List<RoomScheduleOfferingDto> Offerings { get; init; } = new();
  public bool HasConflicts => Offerings.Any(o => o.HasConflict);
}
