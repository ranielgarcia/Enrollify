using Enrollify.Core.Services.NotificationServices;
using Enrollify.Core.Services.NotificationServices.Models;
using Wolverine;

namespace Enrollify.Infrastructure.Services.NotificationServices;

public class NotificationBus : INotificationBus
{
  private readonly IMessageBus _messageBus;

  public NotificationBus(IMessageBus messageBus)
  {
    _messageBus = messageBus;
  }

  public async Task PublishAsync(NotificationCreatedEvent notification)
  {
    await _messageBus.PublishAsync(notification);
  }
}
