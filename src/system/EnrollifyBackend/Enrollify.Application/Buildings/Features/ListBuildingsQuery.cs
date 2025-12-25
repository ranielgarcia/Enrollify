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
    private readonly IReadRepository<Building> _roomRepository;

    public ListBuildingsQueryHandler(IReadRepository<Building> roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async ValueTask<Result<List<BuildingDTO>>> Handle (ListBuildingsQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListBuildingsSpec();
        var buildings = await _roomRepository.ListAsync(spec, cancellationToken);

        var toReturn = buildings
            .Select(BuildingDTO.FromEntity).ToList();

        return Result.Success(toReturn);
    }
}
