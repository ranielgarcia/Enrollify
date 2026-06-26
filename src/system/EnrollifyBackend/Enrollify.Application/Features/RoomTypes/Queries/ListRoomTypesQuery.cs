using Enrollify.Application.Features.RoomTypes.DTOs;
using Enrollify.Application.Features.RoomTypes.Specifications;

namespace Enrollify.Application.Features.RoomTypes.Queries;

public class ListRoomTypesQuery : IRequest<Result<List<RoomTypeDto>>>
{
}

public class ListRoomTypesQueryHandler : IRequestHandler<ListRoomTypesQuery, Result<List<RoomTypeDto>>>
{
    private readonly IReadRepository<RoomType> _repository;

    public ListRoomTypesQueryHandler(IReadRepository<RoomType> repository)
    {
        _repository = repository;
    }
    public async Task<Result<List<RoomTypeDto>>> Handle(ListRoomTypesQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListRoomTypesWithAllNavigationSpec();
        var roomTypes = await _repository.ListAsync(spec, cancellationToken);

        var toReturn = roomTypes
            .Select(RoomTypeDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
