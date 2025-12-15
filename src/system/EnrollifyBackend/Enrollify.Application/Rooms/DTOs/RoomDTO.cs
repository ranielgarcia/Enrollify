using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.Rooms.DTOs;

public class RoomDTO
{
    public string RoomNumber { get; set; }
    public int Capacity { get; set; }
    public RoomTypeId RoomTypeId { get; set; }
    public BuildingId BuildingId { get; set; }
    public CollegeId CollegeId { get; set; }

    public static RoomDTO FromRoomEntity (Room room)
    {
        return new RoomDTO
        {
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            RoomTypeId = room.RoomTypeId,
            BuildingId = room.BuildingId,
            CollegeId = room.CollegeId
        };
    }

}
