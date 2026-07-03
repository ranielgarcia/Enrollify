using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public class RefreshClassSectionDataQualityValidationIssuesRequestedEventHandler(
  IMediator mediator) : IDomainEventHandler<RefreshClassSectionDataQualityValidationIssuesRequestedEvent>
{
  public async Task Handle(RefreshClassSectionDataQualityValidationIssuesRequestedEvent notification, CancellationToken cancellationToken)
  {
    await mediator.Send(new ComputeAndGetDataQualityValidationIssuesForClassSections.Command(notification.ClassSectionIds), cancellationToken);
  }
}
