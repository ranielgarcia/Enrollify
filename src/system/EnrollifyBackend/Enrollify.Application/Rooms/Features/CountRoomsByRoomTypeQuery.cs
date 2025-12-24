using Ardalis.Result;
using Enrollify.Application.Rooms.Specifications;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Rooms.Features;

public class CountRoomsByRoomTypeQuery : IQuery<Result<int>>
{
    public RoomTypeId RoomTypeId { get; set; }
}

public class CountRoomsByRoomTypeQueryHandler (IReadRepository<Room> roomRepository) 
    : IQueryHandler<CountRoomsByRoomTypeQuery, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CountRoomsByRoomTypeQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsByRoomTypeSpec(query.RoomTypeId);
        var count = await roomRepository.CountAsync(spec, cancellationToken);

        return Result<int>.Success(count);
    }
}