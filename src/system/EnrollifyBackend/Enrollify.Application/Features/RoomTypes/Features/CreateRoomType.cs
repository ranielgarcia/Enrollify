using Ardalis.Result;
using Enrollify.Application.Features.RoomTypes;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Mediator;

namespace Enrollify.Application.Features.RoomTypes.Features;

public static class CreateRoomType
{
    public sealed record Command(string name, string description) : ICommand<Result<RoomTypeId>>;

    public sealed class Handler : ICommandHandler<Command, Result<RoomTypeId>>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;

        public Handler(IRoomTypeRepository roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }
        public async ValueTask<Result<RoomTypeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var roomType = new RoomType(command.name, command.description);
            var result = await _roomTypeRepository.Create(roomType, cancellationToken);
            return result;
        }
    }
}
