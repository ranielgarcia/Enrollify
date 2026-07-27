using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Services.ClientDataInvalidation;

namespace Enrollify.Application.Features.ClientDataInvalidations;

public class OnClassSectionMovedToDraftEventHandler (IClientDataInvalidationDispatcher clientDataInvalidationDispatcher)
  : IDomainEventHandler<ClassSectionMovedToDraftEvent>
{
  public async Task Handle(ClassSectionMovedToDraftEvent notification, CancellationToken cancellationToken)
  {
    var targetUserIds = notification.TriggeredBy != null
      ? new[] { notification.TriggeredBy.Value }
      : null;

    await clientDataInvalidationDispatcher.DispatchClientDataInvalidationToTargetUser(
      nameof(ClassSectionMovedToDraftEvent), targetUserIds: targetUserIds, ct: cancellationToken);
  }
}
