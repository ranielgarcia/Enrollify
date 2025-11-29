using Mediator;

namespace Enrollify.SharedKernel;

public interface IDomainEventHandler<T> : INotificationHandler<T> where T : IDomainEvent
{
}
