using Ardalis.Result;
using Enrollify.Application.Features.Buildings;
using Enrollify.Application.Features.Rooms;
using Enrollify.Application.Features.Rooms.Features;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Mediator;

namespace Enrollify.Application.Features.Buildings.Features;

public static class DeleteBuilding
{
    public sealed record Command(BuildingId id) : ICommand<Result>;

    public sealed class Handler : ICommandHandler<Command, Result>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IMediator _mediator;

        public Handler(IBuildingRepository buildingRepository, IMediator mediator)
        {
            _buildingRepository = buildingRepository;
            _mediator = mediator;
        }

        public async ValueTask<Result> Handle (Command command, CancellationToken cancellationToken)
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
