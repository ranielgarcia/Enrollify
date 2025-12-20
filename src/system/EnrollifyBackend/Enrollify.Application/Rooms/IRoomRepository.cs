using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.Rooms;

public interface IRoomRepository
{
    Task<List<Room>> GetAllByRoomType(RoomTypeId roomTypeId, CancellationToken cancellationToken);
}
