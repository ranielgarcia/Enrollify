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

  public ClassSectionSchedulingStats(CollegeId collegeId, CourseId courseId, AcademicTermId academicTermId,
    ClassSectionId? classSectionId, ClassSectionSchedulingStatsAggregateTypeEnum type,
    int aggregateCount)
  {
    CollegeId = Guard.Against.Null(collegeId);
    CourseId = Guard.Against.Null(courseId);
    AcademicTermId = Guard.Against.Null(academicTermId);
    ClassSectionId = classSectionId; // nullable for course-level aggregates
    Type = Guard.Against.Null(type);
    AggregateCount = aggregateCount;
    ComputedAt = DateTimeOffset.UtcNow;
  }

  public CollegeId CollegeId { get; private set; }

  public CourseId CourseId { get; private set; }

  public AcademicTermId AcademicTermId { get; private set; }

  public ClassSectionId? ClassSectionId { get; private set; }

  public ClassSectionSchedulingStatsAggregateTypeEnum Type { get; private set; }
  public int AggregateCount { get; private set; }
  public DateTimeOffset ComputedAt { get; private set; }
}
