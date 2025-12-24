using Ardalis.Specification;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;

namespace Enrollify.Application.Rooms.Specifications;

public class ListRoomsByBuildingSpec : Specification<Room>
{
    public ListRoomsByBuildingSpec(BuildingId buildingId) =>
        Query.Where(room => room.BuildingId == buildingId);
}
