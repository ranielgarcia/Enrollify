using Ardalis.Result;
using Enrollify.Application.Features.Rooms.Specifications;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Rooms.Queries;

public class CountRoomsByRoomTypeQuery : IRequest<Result<int>>
{
    public RoomTypeId RoomTypeId { get; set; }
}

public class CountRoomsByRoomTypeQueryHandler (IReadRepository<Room> roomRepository)
    : IRequestHandler<CountRoomsByRoomTypeQuery, Result<int>>
{
    public async Task<Result<int>> Handle(CountRoomsByRoomTypeQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsByRoomTypeSpec(query.RoomTypeId);
        var count = await roomRepository.CountAsync(spec, cancellationToken);

        return Result<int>.Success(count);
    }
}
