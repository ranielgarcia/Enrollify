using Ardalis.Specification;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers.Specifications;

public class SearchTeachersPaginatedSpec : Specification<Teacher>
{
    public SearchTeachersPaginatedSpec(int pageNumber, int pageSize, string? searchTerm)
    {
        // The explicit cast `((string)s.Code).Contains(searchTerm)` leverages Vogen's generated explicit operator string(SubjectCode) to let EF Core resolve it to the
        // underlying string column. string.Contains then translates to SQL LIKE '%term%'.
        // No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(x => x.Department)
            .AsSplitQuery();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            Query.Where(x =>
                ((string)x.FirstName).Contains(term) ||
                ((string)x.LastName).Contains(term) ||
                (x.MiddleName != null && ((string)x.MiddleName).Contains(term)) ||
                ((string)x.TeacherIdentifier).Contains(term) ||
                ((string)x.Email).Contains(term) ||
                ((string)x.PhoneNumber).Contains(term));
        }

        Query
            .OrderBy(x => new { x.LastName, x.FirstName })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
