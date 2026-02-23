using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class SearchSubjectsPaginatedSpec : Specification<Subject>
{
    public SearchSubjectsPaginatedSpec(int pageNumber, int pageSize, string searchTerm)
    {
        // The explicit cast (string) leverages Vogen's generated explicit operator string(SubjectCode) to let EF Core resolve it to the
        // underlying string column. string.Contains then translates to SQL LIKE '%term%'.
        // No EF Core dependency needed — only Ardalis.Specification.
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType)
            .Where(s => ((string)s.Code).Contains(searchTerm) || s.Title.Contains(searchTerm) || s.Description.Contains(searchTerm))
            .OrderBy(s => s.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
