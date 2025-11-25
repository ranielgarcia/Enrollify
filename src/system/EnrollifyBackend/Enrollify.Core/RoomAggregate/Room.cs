using Ardalis.GuardClauses;
using Enrollify.Core.RoomAggregate.Events;
using Enrollify.Core.RoomTypeAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.RoomAggregate;

public class Room : EntityBase<Room, RoomId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private Room() { } // EF Core constructor

    public Room(RoomName name, int studentCapacity, RoomTypeId roomTypeId)
    {
        Name = Guard.Against.Null(name);
        StudentCapacity = Guard.Against.NegativeOrZero(studentCapacity);
        RoomTypeId = Guard.Against.Null(roomTypeId);

        RegisterDomainEvent(new RoomCreatedEvent(this));
    }

    public AuditInfo AuditInfo { get; set; } = new AuditInfo();
    public RoomName Name { get; private set; }
    public int StudentCapacity { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }

    // Navigation property - not exposed publicly
    private RoomType? _roomType;

    public Room UpdateName(RoomName newName)
    {
        if (Name == newName) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public Room UpdateCapacity(int newCapacity)
    {
        StudentCapacity = Guard.Against.NegativeOrZero(newCapacity);
        return this;
    }

    public Room UpdateRoomType(RoomTypeId newRoomTypeId)
    {
        if (RoomTypeId == newRoomTypeId) return this;
        RoomTypeId = Guard.Against.Null(newRoomTypeId);
        return this;
    }
}