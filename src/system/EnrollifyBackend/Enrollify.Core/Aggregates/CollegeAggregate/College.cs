using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CollegeAggregate;

public class College : EntityBase<College, CollegeId>, IAggregateRoot, IAuditable 
{ 
    private College() { }
    public College(CollegeCode code, string name, string description, string dean)
    {
        Code = Guard.Against.Null(code);
        Name = Guard.Against.Null(name);
        Description = Guard.Against.Null(description);
        Dean = Guard.Against.Null(dean);
    }

    public CollegeCode Code { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Dean { get; private set; } = null!;

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

    public College UpdateCode(CollegeCode newCode)
    {
        if (newCode == Code) return this;
        Code = Guard.Against.Null(newCode);
        return this;
    }

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

    public College UpdateDean(string newDean)
    {
        if (newDean == Dean) return this;
        Dean = Guard.Against.Null(newDean);
        return this;
    }

}
