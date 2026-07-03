namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class GetClassSectionSubjectOfferingWithSchedulesByIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingWithSchedulesByIdSpec(ClassSectionSubjectOfferingId offeringId)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Where(o => o.Id == offeringId);
  }
}
