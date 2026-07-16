namespace Enrollify.SharedKernel;

public interface IDomainEventBus
{
  Task PublishAsync(DomainEventBase domainEvent);

  Task PublishAllAsync(IEnumerable<DomainEventBase> domainEvents);
}
