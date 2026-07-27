using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;

public class RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent : DomainEventBase
{
  public RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(AcademicTermId termId,
    CourseId courseId, ClassSectionId? classSectionId = null, UserId? triggeredBy = null)
  {
    TermId = termId;
    CourseId = courseId;
    ClassSectionId = classSectionId;
    TriggeredBy = triggeredBy;
  }

  public AcademicTermId TermId { get; init; }
  public CourseId CourseId { get; init; }
  public ClassSectionId? ClassSectionId { get; init; }

  /// <summary>
  /// The user who triggered this refresh. Propagated from the originating event so that
  /// background handlers (running outside an HTTP context) know which user to notify.
  /// Null when raised from an HTTP request context — the handler resolves the user from
  /// <c>ICurrentUserAccessor</c> in that case.
  /// </summary>
  public UserId? TriggeredBy { get; init; }
}
