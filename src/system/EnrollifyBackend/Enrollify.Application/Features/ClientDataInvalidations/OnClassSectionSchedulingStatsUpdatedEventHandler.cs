using Enrollify.Application.Features.ClassSectionScheduling.Events;
using Enrollify.Core.Services.ClientDataInvalidation;

namespace Enrollify.Application.Features.ClientDataInvalidations;

public class OnClassSectionSchedulingStatsUpdatedEventHandler (IClientDataInvalidationDispatcher clientDataInvalidationDispatcher) : INotificationHandler<ClassSectionSchedulingStatsUpdatedEvent>
{
  public async Task Handle(ClassSectionSchedulingStatsUpdatedEvent notification, CancellationToken cancellationToken)
  {
    await clientDataInvalidationDispatcher.BroadcastClientDataInvalidation(nameof(ClassSectionSchedulingStatsUpdatedEvent), cancellationToken);
  }
}
