using Enrollify.SharedKernel;

namespace Enrollify.Core.RoomAggregate.Events;

public class RoomCreatedEvent(Room room) : DomainEventBase
{
    public Room Room { get; init; } = room;
}
