namespace Enrollify.Application.Features.RoomScheduling.DTOs;

/// <summary>
/// Summary statistics for the room-schedule grid for the selected term and day.
/// </summary>
public sealed class RoomScheduleStatsDto
{
  public int TotalRooms { get; init; }
  public int RoomsInUse { get; init; }
  public int TotalOfferings { get; init; }
  public int UnassignedOfferings { get; init; }

  /// <summary>Number of distinct room double-booking pairs detected for the day.</summary>
  public int TotalConflicts { get; init; }
}
