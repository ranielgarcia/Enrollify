using Enrollify.Core.Services.NotificationServices;
using Enrollify.Core.Services.NotificationServices.Models;
using Enrollify.Infrastructure.Data;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.Infrastructure.Services.NotificationServices;

public class NotificationBus : INotificationBus
{
  private readonly IDbContextOutbox _outbox;

  public NotificationBus(IDbContextOutbox outbox)
  {
    _outbox = outbox;
  }

  public async Task PublishAsync(NotificationCreatedEvent notification)
  {
    await _outbox.PublishAsync(notification);
  }
}
