namespace Enrollify.Application.Features.Rooms.Specifications;

public class ListRoomsByRoomTypeSpec : Specification<Room>
{
    public ListRoomsByRoomTypeSpec(RoomTypeId roomTypeId) =>
        Query.Where(room => room.RoomTypeId == roomTypeId);
}
