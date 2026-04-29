using Ardalis.Specification;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.Features.Departments.Specifications;

public class ListDepartmentsWithAllNavigationSpec : Specification<Department>
{
    public ListDepartmentsWithAllNavigationSpec() =>
        Query
        .Include(d => d.College)
        .Include(r => r.CreatedByUser)
        .Include(r => r.UpdatedByUser);
}
