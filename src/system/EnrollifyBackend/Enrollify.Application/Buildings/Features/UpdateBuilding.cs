using Ardalis.Result;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public static class UpdateBuilding
{
    public sealed record Command(BuildingId id, string name, string description, string address, CollegeId collegeId) 
        : ICommand<Result<BuildingId>>;

    public sealed class Handler : ICommandHandler<Command, Result<BuildingId>>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IReadRepository<Building> _buildingReadRepository;

        public Handler(IBuildingRepository buildingRepository, IReadRepository<Building> buildingReadRepository)
        {
            _buildingRepository = buildingRepository;
            _buildingReadRepository = buildingReadRepository;
        }

        public async ValueTask<Result<BuildingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await _buildingReadRepository.GetByIdAsync(command.id, cancellationToken);
            if (existing is null)
            {
                return Result.NotFound($"Building with an ID of {command.id.Value} not found.");
            }

            existing.UpdateCollege(command.collegeId);
            existing.UpdateName(command.name);
            existing.UpdateDescription(command.description);
            existing.UpdateAddress(command.address);

            var updateResult = await _buildingRepository.Update(existing, cancellationToken);
            return updateResult;
        }
    }
}
