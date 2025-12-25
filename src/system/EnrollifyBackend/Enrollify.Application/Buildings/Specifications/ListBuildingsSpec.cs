using Ardalis.Specification;
using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Buildings.Specifications;

public class ListBuildingsSpec : Specification<Building>
{
    public ListBuildingsSpec() =>
        Query
        .Include(r => r.CreatedByUser)
        .Include(r => r.UpdatedByUser)
        .Include(r => r.College);
}
