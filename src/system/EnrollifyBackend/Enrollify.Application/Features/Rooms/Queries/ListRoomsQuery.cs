using Enrollify.Application.Features.Rooms.DTOs;
using Enrollify.Application.Features.Rooms.Specifications;

namespace Enrollify.Application.Features.Rooms.Queries;

public class ListRoomsQuery : IRequest<Result<List<RoomDto>>>
{
}

public class ListRoomsQueryHandler(IReadRepository<Room> readRepository)
    : IRequestHandler<ListRoomsQuery, Result<List<RoomDto>>>
{
    public async Task<Result<List<RoomDto>>> Handle(ListRoomsQuery query, CancellationToken cancellationToken)
    {
        var spec = new ListRoomsIncludeAllSpec();
        var rooms = await readRepository.ListAsync(spec, cancellationToken);

        var toReturn = rooms
            .Select(RoomDto.FromProjection).ToList();

        return Result<List<RoomDto>>.Success(toReturn);
    }
}
