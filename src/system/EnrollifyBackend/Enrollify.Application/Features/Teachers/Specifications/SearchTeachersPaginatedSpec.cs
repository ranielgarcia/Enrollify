using Ardalis.Specification;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers.Specifications;

public class SearchTeachersPaginatedSpec : Specification<Teacher>
{
    public SearchTeachersPaginatedSpec(int pageNumber, int pageSize, string? searchTerm)
    {
        // EF Core translates string.Contains(...) to SQL LIKE (depending on provider/collation).
        // Explicit casts are used for Vogen value objects (e.g., TeacherIdentifier) so EF Core can translate to the underlying column type.
        // (Plain string properties like FirstName/LastName don’t need the cast, but keeping it consistent is fine.)
        Query
            .AsNoTracking()
            .Include(x => x.Department)
            .AsSplitQuery();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            Query.Where(x =>
                x.FirstName.Contains(term) ||
                x.LastName.Contains(term) ||
                (x.MiddleName != null && x.MiddleName.Contains(term)) ||
                ((string)x.TeacherIdentifier).Contains(term) ||
                ((string)x.Email).Contains(term) ||
                ((string)x.PhoneNumber).Contains(term));
        }

        Query
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
