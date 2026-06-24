using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionsByIdSpec : Specification<ClassSection>
{
  public GetClassSectionsByIdSpec(List<ClassSectionId> classSectionIds)
  {
    Query.Where(x => classSectionIds.Contains(x.Id));
  }
}
