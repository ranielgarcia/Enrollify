namespace Enrollify.Application.Features.Courses.Specifications;

public class ListCoursesWithAllNavigationSpec : Specification<Course>
{
    public ListCoursesWithAllNavigationSpec()
        => Query
            .Include(c => c.College)
            .Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser);
}
