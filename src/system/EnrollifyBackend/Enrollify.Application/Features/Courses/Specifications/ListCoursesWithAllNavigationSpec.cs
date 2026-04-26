using Ardalis.Specification;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.Courses.Specifications;

public class ListCoursesWithAllNavigationSpec : Specification<Course>
{
    public ListCoursesWithAllNavigationSpec()
        => Query
            .Include(c => c.College);
}
