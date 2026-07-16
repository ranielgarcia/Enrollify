using Enrollify.SharedKernel;
using Wolverine;
using Wolverine.EntityFrameworkCore;

namespace Enrollify.Infrastructure.Services;

public class DomainEventBus (IDbContextOutbox outbox) : IDomainEventBus
{
  public async Task PublishAsync(DomainEventBase domainEvent)
  {
    await outbox.PublishAsync(domainEvent);
  }

  public async Task PublishAllAsync(IEnumerable<DomainEventBase> domainEvents)
  {
    await outbox.PublishAllAsync(domainEvents);
  }
}
