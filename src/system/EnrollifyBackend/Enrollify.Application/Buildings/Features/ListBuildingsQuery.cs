using Ardalis.Result;
using Enrollify.Application.Buildings.DTOs;
using Enrollify.Application.Buildings.Specifications;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Buildings.Features;

public class ListBuildingsQuery : IQuery<Result<List<BuildingDTO>>>
{
}

public class ListBuildingsQueryHandler : IQueryHandler<ListBuildingsQuery, Result<List<BuildingDTO>>>
{
    private readonly IReadRepository<Building> _buildingRepository;

    public ListBuildingsQueryHandler(IReadRepository<Building> buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async ValueTask<Result<List<BuildingDTO>>> Handle (ListBuildingsQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListBuildingsWithAllNavigationSpec();
        var buildings = await _buildingRepository.ListAsync(spec, cancellationToken);

        var toReturn = buildings
            .Select(BuildingDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
