using Ardalis.Specification;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers.Specifications;

public class SearchTeachersPaginatedSpec : Specification<Teacher>
{
    public SearchTeachersPaginatedSpec(int pageNumber, int pageSize, string? searchTerm)
    {
        Query
            .AsNoTracking()
            .Include(t => t.Department);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            //var term = searchTerm.Trim();
            //Query.Where(t =>
            //)
        }

        Query
            .OrderBy(s => new { s.FirstName, s.LastName })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
