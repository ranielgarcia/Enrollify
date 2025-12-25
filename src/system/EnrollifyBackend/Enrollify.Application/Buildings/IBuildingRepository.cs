using Ardalis.Result;
using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Buildings;

public interface IBuildingRepository
{
    Task<Building?> GetById(BuildingId id, CancellationToken cancellationToken);
    Task<Result<BuildingId>> Create (Building newBuilding, CancellationToken cancellationToken);
    Task<Result<BuildingId>> Update (Building newBuilding, CancellationToken cancellationToken);
    Task<Result> Delete (BuildingId id, CancellationToken cancellationToken);
}
