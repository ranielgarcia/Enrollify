using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;

public class GetClassSectionSubjectOfferingsByClassSectionIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsByClassSectionIdSpec(ClassSectionId classSectionId)
  {
    Query.Where(o => o.ClassSectionId == classSectionId && o.IsActive);
  }
}
