using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class ListSubjectsByCodesSpec : Specification<Subject>
{
    public ListSubjectsByCodesSpec(List<SubjectCode> codes)
    {
        Query
            .Where(s => codes.Contains(s.Code));
    }
}
