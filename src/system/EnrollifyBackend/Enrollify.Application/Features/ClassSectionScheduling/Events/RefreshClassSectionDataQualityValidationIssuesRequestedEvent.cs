namespace Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

public class RefreshClassSectionDataQualityValidationIssuesRequestedEvent(List<ClassSectionId> classSectionIds) : DomainEventBase
{
  public List<ClassSectionId> ClassSectionIds { get; init; } = classSectionIds;
}
