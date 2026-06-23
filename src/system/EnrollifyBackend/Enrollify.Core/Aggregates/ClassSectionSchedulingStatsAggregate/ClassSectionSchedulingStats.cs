using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;

public class ClassSectionSchedulingStats : EntityBase<ClassSectionSchedulingStats, ClassSectionSchedulingStatsId>,
  IAggregateRoot
{
  private ClassSectionSchedulingStats()
  {
  }

  public ClassSectionSchedulingStats(AcademicTermId academicTermId, CourseId courseId,
    ClassSectionId? classSectionId, ClassSectionSchedulingStatsAggregateTypeEnum AggregateType,
    int aggregateCount)
  {
    AcademicTermId = Guard.Against.Null(academicTermId);
    CourseId = Guard.Against.Null(courseId);
    ClassSectionId = classSectionId; // nullable for course-level aggregates
    AggregateType = Guard.Against.Null(AggregateType);
    AggregateCount = aggregateCount;
    ComputedAt = DateTimeOffset.UtcNow;
  }

  public AcademicTermId AcademicTermId { get; private set; }
  public CourseId CourseId { get; private set; }


  public ClassSectionId? ClassSectionId { get; private set; }

  public ClassSectionSchedulingStatsAggregateTypeEnum AggregateType { get; private set; }
  public int AggregateCount { get; private set; }
  public DateTimeOffset ComputedAt { get; private set; }
}
