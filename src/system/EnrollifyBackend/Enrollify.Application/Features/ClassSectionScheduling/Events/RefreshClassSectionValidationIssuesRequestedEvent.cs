using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Events;

/// <summary>
/// Raised whenever a change is made to a ClassSection, ClassSectionSubjectOffering, or ClassSchedule
/// that could affect the section's enrollment eligibility. The handler re-runs the full validation
/// pipeline and replaces the stored projection records.
/// </summary>
public class RefreshClassSectionValidationIssuesRequestedEvent(ClassSectionId classSectionId, UserId? triggeredBy = null) : DomainEventBase
{
  public ClassSectionId ClassSectionId { get; init; } = classSectionId;

  /// <summary>
  /// The user who triggered this refresh. Propagated from the originating event so that
  /// background handlers (running outside an HTTP context) know which user to notify.
  /// Null when raised from an HTTP request context — the handler resolves the user from
  /// <c>ICurrentUserAccessor</c> in that case.
  /// </summary>
  public UserId? TriggeredBy { get; init; } = triggeredBy;
}
