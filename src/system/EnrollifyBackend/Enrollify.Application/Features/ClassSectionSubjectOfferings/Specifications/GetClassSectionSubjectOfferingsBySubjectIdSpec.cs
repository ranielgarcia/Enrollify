using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;

public class GetClassSectionSubjectOfferingsBySubjectIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsBySubjectIdSpec(SubjectId subjectId)
  {
    Query.Where(o => o.SubjectId == subjectId && o.IsActive);
  }
}
