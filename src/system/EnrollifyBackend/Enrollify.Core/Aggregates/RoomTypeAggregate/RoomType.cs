using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomTypeAggregate;

public class RoomType : EntityBase<RoomType, RoomTypeId>, IAggregateRoot, IAuditable
{
    private RoomType() { } // EF Core constructor

    public RoomType(string name, string description)
    {
        Name = Guard.Against.Null(name);
        Description = Guard.Against.Null(description);
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;


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