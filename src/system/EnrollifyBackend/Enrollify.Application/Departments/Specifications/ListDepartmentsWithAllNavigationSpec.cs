using Ardalis.Specification;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.Departments.Specifications;

public class ListDepartmentsWithAllNavigationSpec : Specification<Department>
{
    public ListDepartmentsWithAllNavigationSpec() =>
        Query
        .Include(d => d.College)
        .AsSplitQuery();
}
