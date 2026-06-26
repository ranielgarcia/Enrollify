namespace Enrollify.Application.Features.Rooms;

public interface IRoomRepository
{
    Task<Result<RoomId>> Create(Room newRoom, CancellationToken cancellationToken);
    Task<Result<RoomId>> Update(Room newRoom, CancellationToken cancellationToken);
    Task<Result> Delete (RoomId id, CancellationToken cancellationToken);
}
