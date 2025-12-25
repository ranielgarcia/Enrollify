using Ardalis.Result;
using Enrollify.Application.Rooms.Features;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Mediator;

namespace Enrollify.Application.RoomTypes.Features;

public static class DeleteRoomType
{
    public sealed record Command(RoomTypeId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IMediator _mediator;

        public Handler(IRoomTypeRepository roomTypeRepository, IMediator mediator)
        {
            _roomTypeRepository = roomTypeRepository;
            _mediator = mediator;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var countRooms = await _mediator.Send(new CountRoomsByRoomTypeQuery { RoomTypeId = command.id }, cancellationToken);
            if (countRooms.Value > 0)
            {
                return Result.Invalid(new ValidationError($"This room type cannot be deleted because it has {countRooms.Value} room(s) associated with it. \n Please reassign or remove these rooms before deleting."));
            }

            return await _roomTypeRepository.Delete(command.id, cancellationToken);
        }
    }
}
