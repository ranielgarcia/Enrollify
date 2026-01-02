using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class ListSubjectsSpec : Specification<Subject>
{
    public ListSubjectsSpec() =>
        Query
        .Include(s => s.Course)
        .Include(s => s.PreferRoomType);
}
