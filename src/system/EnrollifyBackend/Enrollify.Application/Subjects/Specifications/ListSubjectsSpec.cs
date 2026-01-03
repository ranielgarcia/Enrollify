using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class ListSubjectsSpec : Specification<Subject>
{
    public ListSubjectsSpec(int pageNumber, int pageSize)
    {
        Query
            .Include(s => s.Course)
            .Include(s => s.PreferRoomType)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
