using Ardalis.Specification;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.Features.Rooms.Specifications;

public class ListRoomsByRoomTypeSpec : Specification<Room>
{
    public ListRoomsByRoomTypeSpec(RoomTypeId roomTypeId) =>
        Query.Where(room => room.RoomTypeId == roomTypeId);
}
