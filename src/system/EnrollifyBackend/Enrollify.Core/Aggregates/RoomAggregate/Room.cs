using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.RoomAggregate.Events;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomAggregate;

public class Room : EntityBase<Room, RoomId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private Room() { } // EF Core constructor

    public Room(RoomName name, RoomStudentCapacity studentCapacity, RoomTypeId roomTypeId)
    {
        Name = Guard.Against.Null(name);
        StudentCapacity = studentCapacity;
        RoomTypeId = Guard.Against.Null(roomTypeId);

        RegisterDomainEvent(new RoomCreatedEvent(this));
    }

    public RoomName Name { get; private set; }
    public RoomStudentCapacity StudentCapacity { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    // Navigation property - not exposed publicly
    private RoomType? _roomType;

    public Room UpdateName(RoomName newName)
    {
        if (Name == newName) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public Room UpdateCapacity(RoomStudentCapacity newCapacity)
    {
        if (newCapacity == StudentCapacity) return this;
        StudentCapacity = newCapacity;
        return this;
    }

    public Room UpdateRoomType(RoomTypeId newRoomTypeId)
    {
        if (RoomTypeId == newRoomTypeId) return this;
        RoomTypeId = Guard.Against.Null(newRoomTypeId);
        return this;
    }
}