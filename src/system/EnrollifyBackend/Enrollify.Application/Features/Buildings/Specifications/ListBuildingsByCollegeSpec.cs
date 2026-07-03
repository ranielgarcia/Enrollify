namespace Enrollify.Application.Features.Buildings.Specifications;

public class ListBuildingsByCollegeSpec : Specification<Building>
{
    public ListBuildingsByCollegeSpec(CollegeId collegeId) =>
        Query.Where(b => b.CollegeId == collegeId);
}
