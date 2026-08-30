namespace Enrollify.Application.Features.RoomScheduling.Models;

/// <summary>
/// Flat Dapper projection for a single room in the room-schedule read model.
/// </summary>
public sealed class RoomScheduleRoomRow
{
  public int Id { get; init; }
  public string RoomNumber { get; init; } = string.Empty;
  public int Capacity { get; init; }
  public string? RoomTypeName { get; init; }
  public int? BuildingId { get; init; }
  public string? BuildingName { get; init; }
  public int? CollegeId { get; init; }
  public string? CollegeCode { get; init; }
}
