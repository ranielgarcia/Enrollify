using Ardalis.Specification;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Features.Colleges.Specifications;

public class ListCollegesOrderByNameSpec : Specification<College>
{
    public ListCollegesOrderByNameSpec() => 
        Query
        .Include(r => r.CreatedByUser)
        .Include(r => r.UpdatedByUser)
        .OrderBy(c => c.Name);
}
