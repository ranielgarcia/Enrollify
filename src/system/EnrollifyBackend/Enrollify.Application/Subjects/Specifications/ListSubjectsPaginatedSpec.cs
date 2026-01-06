using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class ListSubjectsPaginatedSpec : Specification<Subject>
{
    public ListSubjectsPaginatedSpec(int pageNumber, int pageSize)
    {
        Query
            .AsNoTracking()
            .Include(s => s.PreferRoomType)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
