using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public class RefreshClassSectionDataIntegrityValidationIssuesRequestedEventHandler(
  IMediator mediator) : IDomainEventHandler<RefreshClassSectionDataIntegrityValidationIssuesRequestedEvent>
{
  public async Task Handle(RefreshClassSectionDataIntegrityValidationIssuesRequestedEvent notification, CancellationToken cancellationToken)
  {
    await mediator.Send(new ComputeAndGetDataIntegrityValidationIssuesForClassSections.Command(notification.ClassSectionIds), cancellationToken);
  }
}
