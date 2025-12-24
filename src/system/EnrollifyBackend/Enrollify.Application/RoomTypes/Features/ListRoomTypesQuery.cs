using Ardalis.Result;
using Enrollify.Application.RoomTypes.DTOs;
using Mediator;

namespace Enrollify.Application.RoomTypes.Features;

public class ListRoomTypesQuery : IQuery<Result<List<RoomTypeDTO>>>
{
}

public class ListRoomTypesQueryHandler : IQueryHandler<ListRoomTypesQuery, Result<List<RoomTypeDTO>>>
{
    private readonly IRoomTypeRepository _repository;

    public ListRoomTypesQueryHandler(IRoomTypeRepository repository)
    {
        _repository = repository;
    }
    public async ValueTask<Result<List<RoomTypeDTO>>> Handle(ListRoomTypesQuery request, CancellationToken cancellationToken)
    {
        var roomTypes = await _repository.ListRoomTypes(cancellationToken);

        var toReturn = roomTypes
            .Select(RoomTypeDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
