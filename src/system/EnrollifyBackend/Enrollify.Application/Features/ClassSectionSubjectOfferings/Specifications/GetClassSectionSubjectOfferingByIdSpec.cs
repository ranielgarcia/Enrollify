using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;

public class GetClassSectionSubjectOfferingByIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingByIdSpec(ClassSectionSubjectOfferingId id)
  {
    Query.Where(c => c.Id == id);
  }
}
