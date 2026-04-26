using Ardalis.Result;
using Enrollify.Application.Features.Buildings.Specifications;
using Enrollify.Application.Features.Buildings.DTOs;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Buildings.Queries;

public class ListBuildingsQuery : IQuery<Result<List<BuildingDto>>>
{
}

public class ListBuildingsQueryHandler : IQueryHandler<ListBuildingsQuery, Result<List<BuildingDto>>>
{
    private readonly IReadRepository<Building> _buildingRepository;

    public ListBuildingsQueryHandler(IReadRepository<Building> buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async ValueTask<Result<List<BuildingDto>>> Handle (ListBuildingsQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListBuildingsWithAllNavigationSpec();
        var buildings = await _buildingRepository.ListAsync(spec, cancellationToken);

        var toReturn = buildings
            .Select(BuildingDto.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
