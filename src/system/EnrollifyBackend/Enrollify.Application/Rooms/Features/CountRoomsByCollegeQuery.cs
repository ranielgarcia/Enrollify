using Ardalis.Result;
using Enrollify.Application.Rooms.Specifications;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Rooms.Features;

public class CountRoomsByCollegeQuery : IQuery<Result<int>>
{
    public CollegeId CollegeId { get; set; }
}

public class CountRoomsByCollegeQueryHandler(IReadRepository<Room> roomRepository) 
    : IQueryHandler<CountRoomsByCollegeQuery, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CountRoomsByCollegeQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsByCollegeSpec(query.CollegeId);
        var count = await roomRepository.CountAsync(spec, cancellationToken);
        return Result<int>.Success(count);
    }
}