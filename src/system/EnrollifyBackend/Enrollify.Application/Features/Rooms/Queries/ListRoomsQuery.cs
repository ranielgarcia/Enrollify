using Ardalis.Result;
using Enrollify.Application.Features.Rooms.DTOs;
using Enrollify.Application.Features.Rooms.Specifications;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Rooms.Queries;

public class ListRoomsQuery : IQuery<Result<List<RoomDto>>>
{
}

public class ListRoomsQueryHandler(IReadRepository<Room> readRepository) 
    : IQueryHandler<ListRoomsQuery, Result<List<RoomDto>>>
{
    public async ValueTask<Result<List<RoomDto>>> Handle(ListRoomsQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsIncludeAllSpec();
        var rooms = await readRepository.ListAsync(spec, cancellationToken);

        var toReturn = rooms
            .Select(RoomDto.FromProjection).ToList();

        return Result<List<RoomDto>>.Success(toReturn);
    }
}
