namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionByIdSpec : Specification<ClassSection>
{
  public GetClassSectionByIdSpec(ClassSectionId classSectionId)
  {
    Query.Where(c => c.Id == classSectionId);
  }
}
