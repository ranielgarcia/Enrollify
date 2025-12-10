using Ardalis.GuardClauses;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CollegeAggregate;

public class College : EntityBase<College, CollegeId>, IAggregateRoot, IAuditable<AuditInfo> 
{ 
    public College() { }
    public College(string name, string description)
    {
        Name = name;
        Description = description;
    }
    public string Name { get; set; }
    public string Description { get; set; }
    public AuditInfo AuditInfo { get; init; } = new AuditInfo();

    public College UpdateName(string newName)
    {
        if (newName == Name) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public College UpdateDescription(string newDescription)
    {
        if (newDescription == Description) return this;
        Description = Guard.Against.Null(newDescription);
        return this;
    }
}
