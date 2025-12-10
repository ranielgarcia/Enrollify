using Ardalis.GuardClauses;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomTypeAggregate;

public class RoomType : EntityBase<RoomType, RoomTypeId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private RoomType() { } // EF Core constructor

    public RoomType(string name)
    {
        Name = Guard.Against.Null(name);
    }

    public string Name { get; private set; }
    public string Description { get; set; }
    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public static RoomType Create(string name, string description)
    {
        var roomType = new RoomType(name);
        roomType.Description = description;
        return roomType;
    }

    public RoomType UpdateName(string newName)
    {
        if (Name == newName) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public RoomType UpdateDescription (string description)
    {
        if (Description == description) return this;
        Description = description;
        return this;
    }
}