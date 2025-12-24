using Ardalis.Specification;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;

namespace Enrollify.Application.Rooms.Specifications;

public class ListRoomsByCollegeSpec : Specification<Room>
{
    public ListRoomsByCollegeSpec(CollegeId collegeId) =>
        Query.Where(room => room.CollegeId == collegeId);
}
