using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;

public class GetClassSectionSubjectOfferingWithSchedulesByIdSpec : Specification<ClassSectionSubjectOffering>
{
  public GetClassSectionSubjectOfferingWithSchedulesByIdSpec(ClassSectionSubjectOfferingId offeringId)
  {
    Query
      .Include(o => o.ClassSchedules.Where(s => s.IsActive))
      .Where(o => o.Id == offeringId);
  }
}
