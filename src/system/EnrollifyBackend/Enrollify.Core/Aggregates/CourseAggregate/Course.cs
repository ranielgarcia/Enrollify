using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CourseAggregate;

public class Course : EntityBase<Course, CourseId>, IAggregateRoot, IAuditable
{
    private Course() { } // EF Core constructor

    public Course(CourseCode code, string name, int durationYears, string description, CollegeId collegeId)
    {
        Code = Guard.Against.Null(code);
        Name = Guard.Against.Null(name);
        DurationYears = Guard.Against.NegativeOrZero(durationYears);
        Description = Guard.Against.Null(description);
        CollegeId = Guard.Against.Null(collegeId);
    }

    public CourseCode Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DurationYears { get; private set; }
    public string Description { get; private set; } = string.Empty;
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


    public Course UpdateCode(CourseCode newCode)
    {
        if (newCode == Code) return this;
        Code = Guard.Against.Null(newCode);
        return this;
    }

    public Course UpdateName(string newName)
    {
        if (newName == Name) return this;
        Name = Guard.Against.Null(newName);
        return this;
    }

    public Course UpdateDurationYears(int durationYears)
    {
        if (durationYears == DurationYears) return this;
        DurationYears = Guard.Against.NegativeOrZero(durationYears);
        return this;
    }

    public Course UpdateDescription(string newDescription)
    {
        if (newDescription == Description) return this;
        Description = Guard.Against.Null(newDescription);
        return this;
    }

    public Course UpdateCollegeId(CollegeId newCollegeId)
    {
        if (newCollegeId == CollegeId) return this;
        CollegeId = Guard.Against.Null(newCollegeId);
        return this;
    }


}
