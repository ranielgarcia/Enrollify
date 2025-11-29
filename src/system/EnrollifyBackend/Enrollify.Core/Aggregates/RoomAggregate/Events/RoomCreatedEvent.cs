using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomAggregate.Events;

public class RoomCreatedEvent(Room room) : DomainEventBase
{
    public Room Room { get; init; } = room;
}
