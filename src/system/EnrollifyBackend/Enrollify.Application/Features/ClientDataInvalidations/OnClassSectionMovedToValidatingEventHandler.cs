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
    await _clientDataInvalidationDispatcher.DispatchClientDataInvalidationToTargetUser(
      nameof(ClassSectionMovedToValidatingEvent), targetUserIds: null, ct: cancellationToken);
  }
}
