using Ardalis.GuardClauses;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.RoomTypeAggregate;

public class RoomType : EntityBase<RoomType, RoomTypeId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private RoomType() { } // EF Core constructor

    public RoomType(RoomTypeName name)
    {
        Name = Guard.Against.Null(name);
    }

    public RoomTypeName Name { get; private set; }
    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public RoomType UpdateName(RoomTypeName newName)
    {
        if (Name == newName) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }
}