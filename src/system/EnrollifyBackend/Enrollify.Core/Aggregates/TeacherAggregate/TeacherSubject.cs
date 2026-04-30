using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

/// <summary>
/// Links subjects to a teacher, indicating which subjects they are qualified to teach or are currently teaching.
/// </summary>
public class TeacherSubject : IAuditable
{
    public TeacherSubject() {} // EF Core constructor

    public TeacherSubject(TeacherId teacherId, SubjectId subjectId)
    {
        TeacherId = Guard.Against.Null(teacherId);
        SubjectId = Guard.Against.Null(subjectId);
    }

    public TeacherSubjectId Id { get; private set; }
    public TeacherId TeacherId { get; private set; }
    public SubjectId SubjectId { get; private set; }

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
}
