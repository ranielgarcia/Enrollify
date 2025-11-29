using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.UserAggregate.Events;

public class UserCreatedEvent (User user) : DomainEventBase
{
    public User User { get; init; } = user;
}
