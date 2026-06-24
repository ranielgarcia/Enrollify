using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;

public class RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent : DomainEventBase
{
  public RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(AcademicTermId termId,
    CourseId courseId, ClassSectionId? classSectionId = null)
  {
    TermId = termId;
    CourseId = courseId;
    ClassSectionId = classSectionId;
  }

  public AcademicTermId TermId { get; init; }
  public CourseId CourseId { get; init; }
  public ClassSectionId? ClassSectionId { get; init; }
}
