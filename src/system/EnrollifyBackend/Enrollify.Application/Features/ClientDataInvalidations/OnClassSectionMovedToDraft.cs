using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Services.ClientDataInvalidation;

namespace Enrollify.Application.Features.ClientDataInvalidations;

public class OnClassSectionMovedToDraft (IClientDataInvalidationDispatcher clientDataInvalidationDispatcher)
  : IDomainEventHandler<ClassSectionMovedToDraftEvent>
{
  public async Task Handle(ClassSectionMovedToDraftEvent notification, CancellationToken cancellationToken)
  {
    await clientDataInvalidationDispatcher.DispatchClientDataInvalidationToTargetUser(
      nameof(ClassSectionMovedToDraftEvent), targetUserIds: null, ct: cancellationToken);
  }
}
