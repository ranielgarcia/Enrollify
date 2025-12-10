using Ardalis.Result;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.RoomTypes;

public static class CreateRoomType
{
    public sealed record Command(string name, string description) : ICommand<Result<RoomTypeId>>;

    public sealed class Handler : ICommandHandler<Command, Result<RoomTypeId>>
    {
        private readonly IRepository<RoomType> _roomTypeRepository;
        public Handler(IRepository<RoomType> roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }
        public async ValueTask<Result<RoomTypeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var roomType = RoomType.Create(command.name, command.description);
            await _roomTypeRepository.AddAsync(roomType, cancellationToken);
            return Result.Success(roomType.Id);
        }
    }
}
