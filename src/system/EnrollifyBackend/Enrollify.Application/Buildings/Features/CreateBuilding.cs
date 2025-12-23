using Ardalis.Result;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public static class CreateBuilding
{
    public sealed record Command(string name, string description, string address) : ICommand<Result<BuildingId>>;

    public sealed class Handler : ICommandHandler<Command, Result<BuildingId>>
    {
        private readonly IBuildingRepository _buildingRepository;

        public Handler(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async ValueTask<Result<BuildingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var building = new Building(command.name, command.description, command.address);
            var result = await _buildingRepository.Create(building, cancellationToken);
            return result;
        }
    }
}
