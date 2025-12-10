using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate.Events;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomAggregate;

public class Room : EntityBase<Room, RoomId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private Room() { } // EF Core constructor

    public Room(string roomNumber, int capacity, RoomTypeId roomTypeId)
    {
        RoomNumber = Guard.Against.Null(roomNumber);
        Capacity = capacity;
        RoomTypeId = Guard.Against.Null(roomTypeId);

        RegisterDomainEvent(new RoomCreatedEvent(this));
    }

    public string RoomNumber { get; private set; }
    public int Capacity { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public BuildingId BuildingId { get; set; }
    public CollegeId CollegeId { get; set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    // Navigation property - not exposed publicly
    private RoomType? _roomType;

    public Room UpdateName(string newRoomNumber)
    {
        if (RoomNumber == newRoomNumber) return this;
        RoomNumber = Guard.Against.Null(newRoomNumber);
        return this;
    }

    public Room UpdateCapacity(int newCapacity)
    {
        if (newCapacity == Capacity) return this;
        Capacity = newCapacity;
        return this;
    }

    public Room UpdateRoomType(RoomTypeId newRoomTypeId)
    {
        if (RoomTypeId == newRoomTypeId) return this;
        RoomTypeId = Guard.Against.Null(newRoomTypeId);
        return this;
    }

    public Room UpdateCollege(CollegeId newCollegeId)
    {
        if (CollegeId == newCollegeId) return this;
        CollegeId = Guard.Against.Null(newCollegeId);
        return this;
    }
}