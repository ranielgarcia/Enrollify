using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomAggregate.Events;
using Enrollify.Core.Aggregates.RoomAggregate.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomAggregate;

public class Room : EntityBase<Room, RoomId>, IAggregateRoot, IAuditable
{
    private Room() { } // EF Core constructor

    public Room(RoomForCreation roomForCreation)
    {
        RoomNumber = Guard.Against.Null(roomForCreation.RoomNumber);
        Capacity = Guard.Against.NegativeOrZero(roomForCreation.Capacity);
        RoomTypeId = Guard.Against.Null(roomForCreation.RoomTypeId);
        BuildingId = Guard.Against.Null(roomForCreation.BuildingId);
        CollegeId = Guard.Against.Null(roomForCreation.CollegeId);

        RegisterDomainEvent(new RoomCreatedEvent(this));
    }

    public string RoomNumber { get; private set; }
    public int Capacity { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public BuildingId BuildingId { get; private set; }
    public CollegeId CollegeId { get; private set; }


    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public User? CreatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public UserId? UpdatedBy { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }
    public User? DeletedByUser { get; private set; }
    public bool IsActive { get; private set; }



    // Navigation property - not exposed publicly
    private RoomType? _roomType;
    public RoomType? RoomType => _roomType;

    private Building? _building;
    public Building? Building => _building;

    private College? _college;
    public College? College => _college;


    public Room UpdateRoomNumber(string newRoomNumber)
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

    public Room UpdateBuilding (BuildingId newBuildingId)
    {
        if (BuildingId == newBuildingId) return this;
        BuildingId = Guard.Against.Null(newBuildingId);
        return this;
    }

    public Room UpdateCollege(CollegeId newCollegeId)
    {
        if (CollegeId == newCollegeId) return this;
        CollegeId = Guard.Against.Null(newCollegeId);
        return this;
    }
}