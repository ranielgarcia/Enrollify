using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.BuildingAggregate;

public class Building : EntityBase<Building, BuildingId>, IAggregateRoot, IAuditable
{
    private Building(){}

    public Building(string name, string description, string address, CollegeId collegeId)
    {
        Name = name;
        Description = description;
        Address = address;
        CollegeId = collegeId;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Address { get; private set; }

    public CollegeId CollegeId { get; private set; }
    public College? College { get; private set; }

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

    public Building UpdateCollege(CollegeId newCollegeId)
    {
        if (newCollegeId == CollegeId) return this;
        CollegeId = newCollegeId;
        return this;
    }

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

    public Building UpdateAddress(string newAddress)
    {
        if (newAddress == Address) return this;
        Address = Guard.Against.Null(newAddress);
        return this;
    }
}
