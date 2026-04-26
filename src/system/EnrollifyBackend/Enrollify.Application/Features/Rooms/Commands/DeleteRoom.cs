using Ardalis.Result;
using Enrollify.Core.Aggregates.RoomAggregate;
using Mediator;

namespace Enrollify.Application.Features.Rooms.Commands;

public static class DeleteRoom
{
    public sealed record Command(RoomId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IRoomRepository _roomRepository;

        public Handler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }
        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // TODO: Add validation if there are other tables / entities associated with the room
            // Prevent room deletion if there are associated records

            return await _roomRepository.Delete(command.id, cancellationToken);
        }
    }
}
