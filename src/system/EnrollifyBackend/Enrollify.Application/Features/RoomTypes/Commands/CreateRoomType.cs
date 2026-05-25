using Ardalis.Result;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using MediatR;

namespace Enrollify.Application.Features.RoomTypes.Commands;

public static class CreateRoomType
{
    public sealed record Command(string name, string description) : IRequest<Result<RoomTypeId>>;

    public sealed class Handler : IRequestHandler<Command, Result<RoomTypeId>>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;

        public Handler(IRoomTypeRepository roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }
        public async Task<Result<RoomTypeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var roomType = new RoomType(command.name, command.description);
            var result = await _roomTypeRepository.Create(roomType, cancellationToken);
            return result;
        }
    }
}
