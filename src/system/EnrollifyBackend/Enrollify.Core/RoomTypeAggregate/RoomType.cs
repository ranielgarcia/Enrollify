using Ardalis.GuardClauses;
using Enrollify.SharedKernel;

namespace Enrollify.Core.RoomTypeAggregate;

public class RoomType : EntityBase<RoomType, RoomTypeId>, IAggregateRoot, IAuditable<AuditInfo>
{
    private RoomType() { } // EF Core constructor

    public RoomType(RoomTypeName name)
    {
        Name = Guard.Against.Null(name);
    }

    public RoomTypeName Name { get; private set; }
    public AuditInfo AuditInfo { get; set; } = new AuditInfo();

    public RoomType UpdateName(RoomTypeName newName)
    {
        if (Name == newName) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }
}