using Ardalis.Result;
using Enrollify.Application.Features.Buildings;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Buildings.Features;

public static class UpdateBuilding
{
    public sealed record Command(BuildingId id, string name, string description, string address, CollegeId collegeId) 
        : ICommand<Result<BuildingId>>;

    public sealed class Handler : ICommandHandler<Command, Result<BuildingId>>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IReadRepository<Building> _buildingReadRepository;
        private readonly IReadRepository<College> _collegeReadRepository;

        public Handler(IBuildingRepository buildingRepository, IReadRepository<Building> buildingReadRepository, 
            IReadRepository<College> collegeReadRepository)
        {
            _buildingRepository = buildingRepository;
            _buildingReadRepository = buildingReadRepository;
            _collegeReadRepository = collegeReadRepository;
        }

        public async ValueTask<Result<BuildingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an id of {command.collegeId} not found");
            }


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
