using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

public class SubjectEquivalence : IAuditable
{
    private SubjectEquivalence() { }

    public SubjectEquivalence(SubjectId subjectId, SubjectEquivalenceGroupId equivalenceGroupId)
    {
        SubjectId = Guard.Against.Null(subjectId);
        EquivalenceGroupId = Guard.Against.Null(equivalenceGroupId);
    }

    public SubjectEquivalenceId Id { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public Subject? Subject { get; private set; }

    public SubjectEquivalenceGroupId EquivalenceGroupId { get; private set; }

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
