using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.DepartmentAggregate;

public class Department : EntityBase<Department, DepartmentId>, IAggregateRoot, IAuditable
{
    private Department() { } // EF Core constructor

    public Department(DepartmentCode code, string name, string chairperson, string description, CollegeId collegeId)
    {
        Code = Guard.Against.Null(code);
        Name = Guard.Against.Null(name);
        Chairperson = Guard.Against.Null(chairperson);
        Description = Guard.Against.Null(description);
        CollegeId = Guard.Against.Null(collegeId);
    }

    public DepartmentCode Code { get; set; }
    public string Name { get; set; }
    public string Chairperson { get; set; }
    public string Description { get; set; }
    public CollegeId CollegeId { get; set; }

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


    public Department UpdateCode(DepartmentCode newCode)
    {
        if (newCode == Code) return this;
        Code = Guard.Against.Null(newCode);
        return this;
    }
    public Department UpdateName(string newName)
    {
        if (newName == Name) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public Department UpdateChairperson(string newChairperson)
    {
        if (newChairperson == Chairperson) return this;
        Chairperson = Guard.Against.Null(newChairperson);
        return this;
    }

    public Department UpdateDescription(string newDescription)
    {
        if (newDescription == Description) return this;
        Description = Guard.Against.Null(newDescription);
        return this;
    }

    public Department UpdateCollegeId(CollegeId newCollegeId)
    {
        if (newCollegeId == CollegeId) return this;
        CollegeId = Guard.Against.Null(newCollegeId);
        return this;
    }


}
