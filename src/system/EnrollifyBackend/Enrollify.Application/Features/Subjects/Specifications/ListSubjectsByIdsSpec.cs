using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.Subjects.Specifications;

public class ListSubjectsByIdsSpec : Specification<Subject>
{
    public ListSubjectsByIdsSpec(List<SubjectId> IDs)
    {
        Query
            .Where(s => IDs.Contains(s.Id));
    }
}
