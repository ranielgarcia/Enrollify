using Ardalis.Specification;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Buildings.Specifications;

public class ListBuildingsByCollegeSpec : Specification<Building>
{
    public ListBuildingsByCollegeSpec(CollegeId collegeId) =>
        Query.Where(b => b.CollegeId == collegeId);
}
