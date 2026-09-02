namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class
  GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(ClassSectionId classSectionId)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Include(o => o.Teacher)
      .Include($"{nameof(ClassSectionSubjectOffering.Room)}.{nameof(Room.Building)}")
      .Include(o => o.CreatedByUser)
      .Include(o => o.UpdatedByUser)
      .Where(o => o.ClassSectionId == classSectionId && o.IsActive);
  }
}
