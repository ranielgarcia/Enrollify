using Ardalis.Result;
using Enrollify.Application.Features.RoomTypes.DTOs;
using Enrollify.Application.Features.RoomTypes.Specifications;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.RoomTypes.Features;

public class ListRoomTypesQuery : IQuery<Result<List<RoomTypeDTO>>>
{
}

public class ListRoomTypesQueryHandler : IQueryHandler<ListRoomTypesQuery, Result<List<RoomTypeDTO>>>
{
    private readonly IReadRepository<RoomType> _repository;

    public ListRoomTypesQueryHandler(IReadRepository<RoomType> repository)
    {
        _repository = repository;
    }
    public async ValueTask<Result<List<RoomTypeDTO>>> Handle(ListRoomTypesQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListRoomTypesWithAllNavigationSpec();
        var roomTypes = await _repository.ListAsync(spec, cancellationToken);

        var toReturn = roomTypes
            .Select(RoomTypeDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
