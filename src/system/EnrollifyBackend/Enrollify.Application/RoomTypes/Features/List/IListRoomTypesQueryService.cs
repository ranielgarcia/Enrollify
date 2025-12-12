using Enrollify.Application.RoomTypes.DTOs;

namespace Enrollify.Application.RoomTypes.Features.List;

public interface IListRoomTypesQueryService
{
    Task<List<RoomTypeDTO>> ListRoomTypesAsync(CancellationToken cancellationToken = default);
}
