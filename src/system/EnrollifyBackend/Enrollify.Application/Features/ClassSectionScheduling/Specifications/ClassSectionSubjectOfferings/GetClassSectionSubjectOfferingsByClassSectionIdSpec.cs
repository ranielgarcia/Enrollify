namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class GetClassSectionSubjectOfferingsByClassSectionIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsByClassSectionIdSpec(ClassSectionId classSectionId)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Where(o => o.ClassSectionId == classSectionId && o.IsActive);
  }

  public GetClassSectionSubjectOfferingsByClassSectionIdSpec(List<ClassSectionId> classSectionIds)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Where(o => classSectionIds.Contains(o.ClassSectionId) && o.IsActive);
  }
}
