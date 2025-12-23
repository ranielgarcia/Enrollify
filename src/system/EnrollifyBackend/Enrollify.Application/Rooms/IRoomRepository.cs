using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.Rooms;

public interface IRoomRepository
{
    Task<List<Room>> GetAllByRoomType(RoomTypeId roomTypeId, CancellationToken cancellationToken);
    Task<List<Room>> GetAllByCollege(CollegeId collegeId, CancellationToken cancellationToken);
    Task<List<Room>> GetAllByBuilding(BuildingId buildingId, CancellationToken cancellationToken);
}
