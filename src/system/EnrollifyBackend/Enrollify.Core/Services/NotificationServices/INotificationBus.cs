using Enrollify.Core.Services.NotificationServices.Models;

namespace Enrollify.Core.Services.NotificationServices;

public interface INotificationBus
{
  Task PublishAsync(NotificationCreatedEvent notification);
}
