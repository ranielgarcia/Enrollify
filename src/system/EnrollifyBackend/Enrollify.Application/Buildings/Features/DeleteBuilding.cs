using Ardalis.Result;
using Enrollify.Application.Rooms;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public static class DeleteBuilding
{
    public sealed record Command(BuildingId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IRoomRepository _roomRepository;

        public Handler(IBuildingRepository buildingRepository, IRoomRepository roomRepository)
        {
            _buildingRepository = buildingRepository;
            _roomRepository = roomRepository;
        }

        public async ValueTask<Result> Handle (Command command, CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.GetAllByBuilding(command.id, cancellationToken);

            if (rooms.Count > 0)
            {
                return Result.Invalid(new ValidationError($"This building cannot be deleted because it has {rooms.Count} room(s) associated with it. \n Please reassign or remove these rooms before deleting."));
            }

            return await _buildingRepository.Delete(command.id, cancellationToken);
        }
    }
}
