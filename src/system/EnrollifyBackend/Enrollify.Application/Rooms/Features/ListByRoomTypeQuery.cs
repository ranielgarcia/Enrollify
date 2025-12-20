using Ardalis.Result;
using Enrollify.Application.Rooms.DTOs;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Mediator;

namespace Enrollify.Application.Rooms.Features;

public class ListByRoomTypeQuery : IQuery<Result<List<RoomDTO>>>
{
    public RoomTypeId RoomTypeId { get; set; }
}

public class ListByRoomTypeQueryHander : IQueryHandler<ListByRoomTypeQuery, Result<List<RoomDTO>>>
{
    private readonly IRoomRepository _roomRepository;

    public ListByRoomTypeQueryHander(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async ValueTask<Result<List<RoomDTO>>> Handle(ListByRoomTypeQuery query, CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.GetAllByRoomType(query.RoomTypeId, cancellationToken);
        var roomsToReturn = rooms.Select(RoomDTO.FromRoomEntity).ToList();
        return roomsToReturn;
    }
}