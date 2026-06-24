using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class
  GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(ClassSectionId classSectionId)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Include(o => o.Teacher)
      .Include(o => o.Room)
      .ThenInclude(r => r.Building)
      .Include(o => o.CreatedByUser)
      .Include(o => o.UpdatedByUser)
      .Where(o => o.ClassSectionId == classSectionId && o.IsActive);
  }
}
