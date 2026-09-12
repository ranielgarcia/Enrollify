using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

public class SubjectEquivalenceGroup : EntityBase<SubjectEquivalenceGroup, SubjectEquivalenceGroupId>, IAggregateRoot, IAuditable
{
    private readonly List<SubjectEquivalence> _subjectEquivalences = new();
    private SubjectEquivalenceGroup() {}

    public SubjectEquivalenceGroup(string name)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
    }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<SubjectEquivalence> SubjectEquivalences => _subjectEquivalences.AsReadOnly();

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

    public SubjectEquivalenceGroup UpdateName (string newName)
    {
        if (Name == newName) return this;
        Name = Guard.Against.NullOrWhiteSpace(newName);
        return this;
    }

    public SubjectEquivalenceGroup AddSubject(SubjectId subjectId)
    {
        if (_subjectEquivalences.Any(se => se.SubjectId == subjectId))
            return this;
        var newEquivalence = new SubjectEquivalence(subjectId, Id);
        _subjectEquivalences.Add(newEquivalence);
        return this;
    }

    public SubjectEquivalenceGroup RemoveSubject (SubjectId subjectId)
    {
        var subjectEquivalence = _subjectEquivalences.FirstOrDefault(se => se.SubjectId == subjectId);
        if (subjectEquivalence != null)
            _subjectEquivalences.Remove(subjectEquivalence);
        return this;
    }

    public IEnumerable<SubjectId> GetSubjects()
    {
        return _subjectEquivalences.Select(se => se.SubjectId);
    }
}
