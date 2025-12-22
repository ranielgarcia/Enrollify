using Ardalis.Result;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Mediator;

namespace Enrollify.Application.RoomTypes.Features;

public static class UpdateRoomType
{
    public sealed record Command(RoomTypeId id, string name, string description) : ICommand<Result<RoomTypeId>>;
    public sealed class Handler : ICommandHandler<Command, Result<RoomTypeId>>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        public Handler(IRoomTypeRepository roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }
        public async ValueTask<Result<RoomTypeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existingRoomType = await _roomTypeRepository.GetById(command.id, cancellationToken);
            if (existingRoomType == null)
            {
                return Result.NotFound($"Room type with an ID of {command.id.Value} not found.");
            }

            existingRoomType.UpdateName(command.name);
            existingRoomType.UpdateDescription(command.description);

            var updateResult = await _roomTypeRepository.Update(existingRoomType, cancellationToken);
            return updateResult;
        }
    }
}
