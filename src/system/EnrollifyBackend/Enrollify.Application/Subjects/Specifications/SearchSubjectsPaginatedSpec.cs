using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Enrollify.Application.Subjects.Specifications;

public class SearchSubjectsPaginatedSpec : Specification<Subject>
{
    public SearchSubjectsPaginatedSpec(int pageNumber, int pageSize, string searchTerm) =>
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType)
            .Where(s => EF.Property<string>(s, nameof(Subject.Code)).Contains(searchTerm) || s.Title.Contains(searchTerm) || s.Description.Contains(searchTerm))
            .OrderBy(s => new { s.Title , s.Code})
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
}
