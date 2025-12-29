using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Aggregates.SubjectAggregate;

public class SubjectPrerequisite : IAuditable
{
    private SubjectPrerequisite() { }

    public SubjectPrerequisite(SubjectId sourceSubjectId, SubjectId prerequisiteSubjectId)
    {
        SourceSubjectId = sourceSubjectId;
        PrerequisiteSubjectId = prerequisiteSubjectId;
    }

    public SubjectId SourceSubjectId { get; private set; }
    public SubjectId PrerequisiteSubjectId { get; private set; }


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
