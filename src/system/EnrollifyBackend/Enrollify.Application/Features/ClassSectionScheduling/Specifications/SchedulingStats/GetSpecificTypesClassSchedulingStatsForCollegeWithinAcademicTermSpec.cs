using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.SchedulingStats;

public class
  GetSpecificTypesClassSchedulingStatsForCollegeWithinAcademicTermSpec : Specification<ClassSectionSchedulingStats>
{
  public GetSpecificTypesClassSchedulingStatsForCollegeWithinAcademicTermSpec(
    List<CourseId> courseIds, AcademicTermId termId,
    List<ClassSectionSchedulingStatsAggregateTypeEnum> statsTypes)
  {
    Query
      .Where(x => courseIds.Contains(x.CourseId) && x.AcademicTermId == termId && statsTypes.Contains(x.AggregateType));
  }
}
