using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.SchedulingStats;

public class GetClassSchedulingStatsForCollegeWithinAcademicTermSpec : Specification<ClassSectionSchedulingStats>
{
  public GetClassSchedulingStatsForCollegeWithinAcademicTermSpec(List<CourseId> courseIds, AcademicTermId termId)
  {
    Query
      .Where(x => courseIds.Contains(x.CourseId) && x.AcademicTermId == termId);
  }
}
