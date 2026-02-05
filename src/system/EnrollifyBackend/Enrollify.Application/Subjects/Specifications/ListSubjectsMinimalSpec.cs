using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class ListSubjectsMinimalSpec : Specification<Subject>
{
    public ListSubjectsMinimalSpec() =>
        Query.AsNoTracking();
}
