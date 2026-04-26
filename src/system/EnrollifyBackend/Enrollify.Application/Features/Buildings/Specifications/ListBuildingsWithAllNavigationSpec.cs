using Ardalis.Specification;
using Enrollify.Core.Aggregates.BuildingAggregate;

namespace Enrollify.Application.Features.Buildings.Specifications;

public class ListBuildingsWithAllNavigationSpec : Specification<Building>
{
    public ListBuildingsWithAllNavigationSpec() =>
        Query
        .Include(r => r.CreatedByUser)
        .Include(r => r.UpdatedByUser)
        .Include(r => r.College);
}
