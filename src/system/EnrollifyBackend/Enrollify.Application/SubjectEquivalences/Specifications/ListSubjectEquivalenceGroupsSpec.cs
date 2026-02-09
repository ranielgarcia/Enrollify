using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.SubjectEquivalences.Specifications;

public class ListSubjectEquivalenceGroupsSpec : Specification<SubjectEquivalenceGroup>
{
    public ListSubjectEquivalenceGroupsSpec() =>
        Query
        .AsNoTracking()
        .AsSplitQuery()
        .Include(seg => seg.SubjectEquivalences).ThenInclude(se => se.Subject);
}
