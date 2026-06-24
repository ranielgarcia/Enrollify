using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

/// <summary>
/// Raised whenever a change is made to a ClassSection, ClassSectionSubjectOffering, or ClassSchedule
/// that could affect the section's enrollment eligibility. The handler re-runs the full validation
/// pipeline and replaces the stored projection records.
/// </summary>
public class RefreshClassSectionValidationIssuesRequestedEvent(ClassSectionId classSectionId) : DomainEventBase
{
  public ClassSectionId ClassSectionId { get; init; } = classSectionId;
}
