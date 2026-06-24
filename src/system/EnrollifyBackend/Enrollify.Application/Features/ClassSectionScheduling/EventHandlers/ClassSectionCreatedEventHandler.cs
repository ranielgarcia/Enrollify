using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.EventHandlers;

public sealed class ClassSectionCreatedEventHandler(
  IMediator mediator)
  : IDomainEventHandler<ClassSectionCreatedEvent>
{
  public async Task Handle(ClassSectionCreatedEvent notification, CancellationToken cancellationToken)
  {
    await mediator.Publish(new RefreshClassSectionValidationIssuesRequestedEvent(notification.Id),
      cancellationToken);
  }
}
