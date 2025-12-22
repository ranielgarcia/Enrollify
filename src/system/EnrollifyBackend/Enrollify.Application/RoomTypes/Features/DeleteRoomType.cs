using Ardalis.Result;
using Enrollify.Application.Rooms;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Mediator;

namespace Enrollify.Application.RoomTypes.Features;

public static class DeleteRoomType
{
    public sealed record Command(RoomTypeId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IRoomRepository _roomRepository;

        public Handler(IRoomTypeRepository roomTypeRepository, IRoomRepository roomRepository)
        {
            _roomTypeRepository = roomTypeRepository;
            _roomRepository = roomRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.GetAllByRoomType(command.id, cancellationToken);

            if (rooms.Count > 0)
            {
                return Result.Invalid(new ValidationError($"This room type cannot be deleted because it has {rooms.Count} room(s) associated with it. \n Please reassign or remove these rooms from this room type before deleting."));
            }

            return await _roomTypeRepository.Delete(command.id, cancellationToken);
        }
    }
}
