using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;

public class GetOfferingsBySubjectIdSpec : Specification<ClassSectionSubjectOffering>
{
    public GetOfferingsBySubjectIdSpec(SubjectId subjectId)
    {
        Query.Where(o => o.SubjectId == subjectId && o.IsActive);
    }
}
