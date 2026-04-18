using Ardalis.Result;
using Enrollify.Application.Features.Rooms;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomAggregate.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Rooms.Features;

public static class CreateRoom
{
    public sealed record Command(string roomNumber, int capacity, RoomTypeId roomTypeId, BuildingId buildingId)
        : ICommand<Result<RoomId>>;

    public sealed class Handler : ICommandHandler<Command, Result<RoomId>>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;
        private readonly IReadRepository<Building> _buildingReadRepository;

        public Handler(IRoomRepository roomRepository, 
            IReadRepository<RoomType> roomTypeReadRepository,
            IReadRepository<Building> buildingReadRepository)
        {
            _roomRepository = roomRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
            _buildingReadRepository = buildingReadRepository;
        }

        public async ValueTask<Result<RoomId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var roomType = await _roomTypeReadRepository.GetByIdAsync(command.roomTypeId, cancellationToken);
            var building = await _buildingReadRepository.GetByIdAsync(command.buildingId, cancellationToken);

            if (roomType == null)
            {
                return Result.NotFound($"Room type with ID {command.roomTypeId} not found.");
            }

            if (building == null)
            {
                return Result.NotFound($"Building with ID {command.buildingId} not found.");
            }

            var newRoom = new Room(new RoomForCreation
            {
                RoomNumber = command.roomNumber,
                Capacity = command.capacity,
                RoomTypeId = command.roomTypeId,
                BuildingId = command.buildingId
            });
            return await _roomRepository.Create(newRoom, cancellationToken);
        }
    }
}
