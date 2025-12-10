using Ardalis.GuardClauses;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.BuildingAggregate;

public class Building : EntityBase<Building, BuildingId>, IAggregateRoot, IAuditable<AuditInfo>
{
    public Building(){}

    public Building(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; }
    public string Description { get; set; }

    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public Building UpdateName (string newName)
    {
        if (newName == Name) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public Building UpdateDescription(string newDescription)
    {
        if (newDescription == Description) return this;
        Description = Guard.Against.Null(newDescription);
        return this;
    }
}
