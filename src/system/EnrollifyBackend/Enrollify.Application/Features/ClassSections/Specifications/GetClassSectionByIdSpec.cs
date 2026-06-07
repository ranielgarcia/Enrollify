using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class GetClassSectionByIdSpec : Specification<ClassSection>
{
  public GetClassSectionByIdSpec(ClassSectionId classSectionId)
  {
    Query.Where(c => c.Id == classSectionId);
  }
}
