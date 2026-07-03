using Enrollify.Application.Features.Rooms.Specifications;

namespace Enrollify.Application.Features.Rooms.Queries;

public class CountRoomsByBuildingQuery : IRequest<Result<int>>
{
    public BuildingId BuildingId { get; set; }
}

public class CountRoomsByBuildingQueryHandler(IReadRepository<Room> roomRepository)
    : IRequestHandler<CountRoomsByBuildingQuery, Result<int>>
{
    public async Task<Result<int>> Handle(CountRoomsByBuildingQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsByBuildingSpec(query.BuildingId);
        var count =  await roomRepository.CountAsync(spec, cancellationToken);
        return Result<int>.Success(count);
    }
}
