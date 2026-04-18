using Ardalis.Result;
using Enrollify.Application.Features.RoomTypes;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.RoomTypes.Features;

public static class UpdateRoomType
{
    public sealed record Command(RoomTypeId id, string name, string description) : ICommand<Result<RoomTypeId>>;
    public sealed class Handler : ICommandHandler<Command, Result<RoomTypeId>>
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IReadRepository<RoomType> _roomTypeReadRepository;

        public Handler(IRoomTypeRepository roomTypeRepository, IReadRepository<RoomType> roomTypeReadRepository)
        {
            _roomTypeRepository = roomTypeRepository;
            _roomTypeReadRepository = roomTypeReadRepository;
        }
        public async ValueTask<Result<RoomTypeId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await _roomTypeReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing == null)
            {
                return Result.NotFound($"Room type with an ID of {command.id.Value} not found.");
            }

            existing.UpdateName(command.name);
            existing.UpdateDescription(command.description);

            var updateResult = await _roomTypeRepository.Update(existing, cancellationToken);
            return updateResult;
        }
    }
}
