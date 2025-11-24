using Mediator;

namespace Enrollify.SharedKernel;

public interface IDomainEvent : INotification
{
  DateTime DateOccurred { get; }
}
