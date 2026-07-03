namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class GetMinimalClassSectionSubjectOfferingsByClassSectionIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetMinimalClassSectionSubjectOfferingsByClassSectionIdSpec(ClassSectionId classSectionId)
  {
    Query
      .Where(o => o.ClassSectionId == classSectionId && o.IsActive);
  }
}
