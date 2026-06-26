using Enrollify.Application.Features.Rooms;
using Enrollify.Application.Features.Rooms.Queries;

namespace Enrollify.Application.Features.Buildings.Commands;

public static class DeleteBuilding
{
    public sealed record Command(BuildingId id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IMediator _mediator;

        public Handler(IBuildingRepository buildingRepository, IMediator mediator)
        {
            _buildingRepository = buildingRepository;
            _mediator = mediator;
        }

        public async Task<Result> Handle (Command command, CancellationToken cancellationToken)
        {
            var countRooms = await _mediator.Send(new CountRoomsByBuildingQuery { BuildingId = command.id }, cancellationToken);

            if (countRooms.Value > 0)
            {
                return Result.Invalid(new ValidationError($"This building cannot be deleted because it has {countRooms.Value} room(s) associated with it. \n Please reassign or remove these rooms before deleting."));
            }

            return await _buildingRepository.Delete(command.id, cancellationToken);
        }
    }
}
