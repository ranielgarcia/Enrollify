using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CollegeAggregate;

public class College : EntityBase<College, CollegeId>, IAggregateRoot, IAuditable 
{ 
    public College() { }
    public College(string name, string description)
    {
        Name = name;
        Description = description;
    }
    public string Name { get; set; }
    public string Description { get; set; }


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
