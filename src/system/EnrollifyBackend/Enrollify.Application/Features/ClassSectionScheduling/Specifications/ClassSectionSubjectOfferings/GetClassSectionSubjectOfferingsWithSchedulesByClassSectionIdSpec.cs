using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

public class
  GetClassSectionSubjectOfferingsWithSchedulesByClassSectionIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingsWithSchedulesByClassSectionIdSpec(ClassSectionId classSectionId)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Where(o => o.ClassSectionId == classSectionId && o.IsActive);
  }
}
