using Ardalis.Result;
using Enrollify.Application.RoomTypes.DTOs;
using Mediator;

namespace Enrollify.Application.RoomTypes.Features.List;

public class ListRoomTypesQuery : IQuery<Result<List<RoomTypeDTO>>>
{
}

public class ListRoomTypesQueryHandler : IQueryHandler<ListRoomTypesQuery, Result<List<RoomTypeDTO>>>
{
    private readonly IListRoomTypesQueryService _queryService;
    public ListRoomTypesQueryHandler(IListRoomTypesQueryService queryService)
    {
        _queryService = queryService;
    }
    public async ValueTask<Result<List<RoomTypeDTO>>> Handle(ListRoomTypesQuery request, CancellationToken cancellationToken)
    {
        var roomTypes = await _queryService.ListRoomTypesAsync(cancellationToken);
        return Result.Success(roomTypes);
    }
}
