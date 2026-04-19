using Ardalis.Result;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Buildings.Commands;

public static class CreateBuilding
{
    public sealed record Command(string name, string description, string address, CollegeId collegeId) : ICommand<Result<BuildingId>>;

    public sealed class Handler : ICommandHandler<Command, Result<BuildingId>>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IReadRepository<College> _collegeReadRepository;

        public Handler(IBuildingRepository buildingRepository, IReadRepository<College> collegeReadRepository)
        {
            _buildingRepository = buildingRepository;
            _collegeReadRepository = collegeReadRepository;
        }

        public async ValueTask<Result<BuildingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var college = await _collegeReadRepository.GetByIdAsync(command.collegeId, cancellationToken);
            if (college == null)
            {
                return Result.NotFound($"College with an id of {command.collegeId} not found");
            }

            var building = new Building(command.name, command.description, command.address, command.collegeId);
            var result = await _buildingRepository.Create(building, cancellationToken);
            return result;
        }
    }
}
