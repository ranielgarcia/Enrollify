namespace Enrollify.Application.Features.Rooms.Specifications;

public class ListRoomsByBuildingSpec : Specification<Room>
{
    public ListRoomsByBuildingSpec(BuildingId buildingId) =>
        Query.Where(room => room.BuildingId == buildingId);
}
