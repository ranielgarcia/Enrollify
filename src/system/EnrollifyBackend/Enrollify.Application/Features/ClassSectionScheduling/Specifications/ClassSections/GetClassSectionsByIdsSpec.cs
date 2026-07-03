namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionsByIdsSpec : Specification<ClassSection>
{
  public GetClassSectionsByIdsSpec(IEnumerable<ClassSectionId> classSectionIds)
  {
    Query.Where(c => classSectionIds.Contains(c.Id));
  }
}
