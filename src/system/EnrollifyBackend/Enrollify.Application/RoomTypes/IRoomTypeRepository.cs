using Ardalis.Result;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Application.RoomTypes;

public interface IRoomTypeRepository
{
    Task<RoomType?> GetById(RoomTypeId id, CancellationToken cancellationToken);
    Task<List<RoomType>> ListRoomTypes(CancellationToken cancellationToken = default);
    Task<Result<RoomTypeId>> Create(RoomType newRoomType, CancellationToken cancellationToken);
    Task<Result<RoomTypeId>> Update(RoomType newRoomType, CancellationToken cancellationToken);
    Task<Result> Delete(RoomTypeId id, CancellationToken cancellation);
}
