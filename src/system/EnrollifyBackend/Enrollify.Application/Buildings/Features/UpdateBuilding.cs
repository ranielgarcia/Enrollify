using Ardalis.Result;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public static class UpdateBuilding
{
    public sealed record Command(BuildingId id, string name, string description, string address) : ICommand<Result<BuildingId>>;

    public sealed class Handler : ICommandHandler<Command, Result<BuildingId>>
    {
        private readonly IBuildingRepository _buildingRepository;

        public Handler(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async ValueTask<Result<BuildingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await _buildingRepository.GetById(command.id, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound($"Building with an ID of {command.id.Value} not found.");
            }

            existing.UpdateName(command.name);
            existing.UpdateDescription(command.description);
            existing.UpdateAddress(command.address);

            var updateResult = await _buildingRepository.Update(existing, cancellationToken);
            return updateResult;
        }
    }
}
