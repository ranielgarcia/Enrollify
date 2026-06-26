namespace Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

public class RefreshClassSectionDataIntegrityValidationIssuesRequestedEvent(List<ClassSectionId> classSectionIds) : DomainEventBase
{
  public List<ClassSectionId> ClassSectionIds { get; init; } = classSectionIds;
}
