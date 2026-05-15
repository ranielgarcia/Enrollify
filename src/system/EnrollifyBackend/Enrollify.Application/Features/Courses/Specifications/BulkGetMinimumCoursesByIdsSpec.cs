using Ardalis.Specification;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.Courses.Specifications;

public class BulkGetMinimumCoursesByIdsSpec : Specification<Course>
{
    public BulkGetMinimumCoursesByIdsSpec(List<CourseId> courseIDs)
    {
        var distinctIds = courseIDs.Distinct().ToList();
        Query.Where(c => distinctIds.Contains(c.Id));
    }
}
