using Ardalis.Result;
using Enrollify.Application.Rooms.Specifications;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Rooms.Features;

public class CountRoomsByBuildingQuery : IQuery<Result<int>>
{
    public BuildingId BuildingId { get; set; }
}

public class CountRoomsByBuildingQueryHandler(IReadRepository<Room> roomRepository) 
    : IQueryHandler<CountRoomsByBuildingQuery, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CountRoomsByBuildingQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsByBuildingSpec(query.BuildingId);
        var count =  await roomRepository.CountAsync(spec, cancellationToken);
        return Result<int>.Success(count);
    }
}