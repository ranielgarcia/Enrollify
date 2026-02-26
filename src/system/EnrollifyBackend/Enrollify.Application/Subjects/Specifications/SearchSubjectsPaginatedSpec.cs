using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class SearchSubjectsPaginatedSpec : Specification<Subject>
{
    public SearchSubjectsPaginatedSpec(int pageNumber, int pageSize, string? searchTerm)
    {
        // The explicit cast `((string)s.Code).Contains(searchTerm)` leverages Vogen's generated explicit operator string(SubjectCode) to let EF Core resolve it to the
        // underlying string column. string.Contains then translates to SQL LIKE '%term%'.
        // No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            Query.Where(s =>
                ((string)s.Code).Contains(term) ||
                s.Title.Contains(term) ||
                s.Description.Contains(term));
        }

        Query
            .OrderBy(s => s.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
