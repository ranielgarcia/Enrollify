namespace Enrollify.Application.Features.Rooms.Commands;

public static class UpdateRoom
{
    public sealed record Command(RoomId id, string roomNumber, int capacity, RoomTypeId roomTypeId, BuildingId buildingId)
        : IRequest<Result<RoomId>>;

    public sealed class Handler : IRequestHandler<Command, Result<RoomId>>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IReadRepository<Room> _roomReadRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;
        private readonly IReadRepository<Building> _buildingReadRepository;

        public Handler(IRoomRepository roomRepository,
            IReadRepository<Room> roomReadRepository,
            IReadRepository<RoomType> roomTypeReadRepository,
            IReadRepository<Building> buildingReadRepository)
        {
            _roomRepository = roomRepository;
            _roomReadRepository = roomReadRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
            _buildingReadRepository = buildingReadRepository;
        }

        public async Task<Result<RoomId>> Handle(Command command, CancellationToken cancellationToken)
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

            var existing = await _roomReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing == null)
            {
                return Result.NotFound($"Room with an ID of {command.id.Value} not found");
            }

            existing.UpdateCapacity(command.capacity);
            existing.UpdateRoomNumber(command.roomNumber);
            existing.UpdateBuilding(building.Id);
            existing.UpdateRoomType(roomType.Id);

            var updatedResult = await _roomRepository.Update(existing, cancellationToken);
            return updatedResult;

        }
    }
}
