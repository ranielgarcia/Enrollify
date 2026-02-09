using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class SearchSubjectsPaginatedSpec : Specification<Subject>
{
    public SearchSubjectsPaginatedSpec(int pageNumber, int pageSize, string searchTerm) =>
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType)
            .Where(s => s.Code == SubjectCode.From(searchTerm) || s.Title.Contains(searchTerm) || s.Description.Contains(searchTerm))
            .OrderBy(s => new { s.Title , s.Code})
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
}
