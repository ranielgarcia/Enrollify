namespace Enrollify.Application.Features.ClassSectionScheduling.Events;

public class RefreshClassSectionDataQualityValidationIssuesRequestedEvent(List<ClassSectionId> classSectionIds) : DomainEventBase
{
  public List<ClassSectionId> ClassSectionIds { get; init; } = classSectionIds;
}
