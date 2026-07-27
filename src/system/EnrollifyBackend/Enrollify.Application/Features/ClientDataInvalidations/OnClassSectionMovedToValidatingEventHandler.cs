using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Services.ClientDataInvalidation;

namespace Enrollify.Application.Features.ClientDataInvalidations;

public class OnClassSectionMovedToValidatingEventHandler : IDomainEventHandler<ClassSectionMovedToValidatingEvent>
{
  private readonly IClientDataInvalidationDispatcher _clientDataInvalidationDispatcher;

  public OnClassSectionMovedToValidatingEventHandler(IClientDataInvalidationDispatcher clientDataInvalidationDispatcher)
  {
    _clientDataInvalidationDispatcher = clientDataInvalidationDispatcher;
  }

  public async Task Handle(ClassSectionMovedToValidatingEvent notification, CancellationToken cancellationToken)
  {
    var targetUserIds = notification.TriggeredBy != null
      ? new[] { notification.TriggeredBy.Value }
      : null;

    await _clientDataInvalidationDispatcher.DispatchClientDataInvalidationToTargetUser(
      nameof(ClassSectionMovedToValidatingEvent), targetUserIds: targetUserIds, ct: cancellationToken);
  }
}
