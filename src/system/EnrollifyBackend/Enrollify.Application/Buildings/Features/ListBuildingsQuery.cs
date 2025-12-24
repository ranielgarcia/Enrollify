using Ardalis.Result;
using Enrollify.Application.Buildings.DTOs;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public class ListBuildingsQuery : IQuery<Result<List<BuildingDTO>>>
{
}

public class ListBuildingsQueryHandler : IQueryHandler<ListBuildingsQuery, Result<List<BuildingDTO>>>
{
    private readonly IBuildingRepository _buildingRepository;

    public ListBuildingsQueryHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async ValueTask<Result<List<BuildingDTO>>> Handle (ListBuildingsQuery request, CancellationToken cancellationToken)
    {
        var buildings = await _buildingRepository.ListBuildings(cancellationToken);

        var toReturn = buildings
            .Select(BuildingDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
