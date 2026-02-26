using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.SubjectEquivalences.Specifications;

public class GetSubjectEquivalenceGroupByIdWithSubjectsSpec : Specification<SubjectEquivalenceGroup>
{
    public GetSubjectEquivalenceGroupByIdWithSubjectsSpec(SubjectEquivalenceGroupId id) =>
        Query
        .Include(seg => seg.SubjectEquivalences.Where(se => se.IsActive)).ThenInclude(se => se.Subject)
        .Where(seg => seg.Id == id);
}
