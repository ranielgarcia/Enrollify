using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;

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
