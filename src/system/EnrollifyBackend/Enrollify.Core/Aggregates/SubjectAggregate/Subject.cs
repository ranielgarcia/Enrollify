using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.SubjectAggregate;

public class Subject : EntityBase<Subject, SubjectId>, IAggregateRoot, IAuditable
{
    private Subject() { }// EF Core constructor


    public Subject(SubjectForCreation newSubject)
    {
        Code = Guard.Against.Null(newSubject.Code);
        Title = Guard.Against.NullOrWhiteSpace(newSubject.Title);
        Units = Guard.Against.NegativeOrZero(newSubject.Units);
        Description = Guard.Against.NullOrWhiteSpace(newSubject.Description);
        CourseId = Guard.Against.Null(newSubject.CourseId);
        PreferRoomTypeId = Guard.Against.Null(newSubject.PreferRoomTypeId);
    }

    public SubjectCode Code { get; private set; }
    public string Title { get; private set; }
    public decimal Units { get; private set; }
    public string Description { get; private set; }
    public CourseId CourseId { get; private set; }
    public Course? Course { get; private set; } = null;

    public RoomTypeId PreferRoomTypeId { get; private set; }
    public RoomType? PreferRoomType { get; private set; }

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

    public Subject UpdateCode(SubjectCode newCode)
    {
        if (Code == newCode) return this;
        Code = Guard.Against.Null(newCode);
        return this;
    }

    public Subject UpdateTitle(string newTitle)
    {
        if (Title == newTitle) return this;
        Title = Guard.Against.NullOrWhiteSpace(newTitle);
        return this;
    }

    public Subject UpdateUnits(decimal newUnits)
    {
        if (Units == newUnits) return this;
        Units = Guard.Against.NegativeOrZero(newUnits);
        return this;
    }

    public Subject UpdateDescription(string newDescription)
    {
        if (Description == newDescription) return this;
        Description = Guard.Against.NullOrWhiteSpace(newDescription);
        return this;
    }

    public Subject UpdateCourse(CourseId newCourseId)
    {
        if (CourseId == newCourseId) return this;
        CourseId = Guard.Against.Null(newCourseId);
        return this;
    }

    public Subject UpdatePreferRoomType(RoomTypeId newRoomTypeId)
    {
        if (PreferRoomTypeId == newRoomTypeId) return this;
        PreferRoomTypeId = Guard.Against.Null(newRoomTypeId);
        return this;
    }
}
