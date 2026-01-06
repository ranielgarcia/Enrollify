using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Aggregates.CurriculaAggregate;

/// <summary>
/// Links subjects to a specific curriculum with year/semester placement.
/// This is where subjects become part of a course's curriculum.
/// </summary>
public class CurriculumSubject : IAuditable
{
    private readonly List<CurriculumSubjectPrerequisite> _prerequisites = new();

    private CurriculumSubject() { } // EF Core constructor

    public CurriculumSubject(
        CurriculaId curriculumId,
        SubjectId subjectId,
        int yearLevel,
        int semester,
        bool isElective,
        string? electiveGroupName,
        UserId addedBy)
    {
        CurriculumId = curriculumId;
        SubjectId = subjectId;
        YearLevel = Guard.Against.OutOfRange(yearLevel, nameof(yearLevel), 1, 6);
        Semester = Guard.Against.OutOfRange(semester, nameof(semester), 1, 3);
        IsElective = isElective;
        ElectiveGroupName = electiveGroupName;
        CreatedBy = addedBy;
    }

    public CurriculumSubjectId Id { get; private set; }
    public CurriculaId CurriculumId { get; private set; }
    public SubjectId SubjectId { get; private set; }

    /// <summary>
    /// Which year this subject is typically taken (1-6)
    /// </summary>
    public int YearLevel { get; private set; }

    /// <summary>
    /// Which semester (1 = First, 2 = Second, 3 = Summer)
    /// </summary>
    public int Semester { get; private set; }

    /// <summary>
    /// Whether this is an elective slot
    /// </summary>
    public bool IsElective { get; private set; }

    /// <summary>
    /// Group name for electives (e.g., 'Major Elective', 'Free Elective')
    /// </summary>
    public string? ElectiveGroupName { get; private set; }

    public Subject? Subject { get; private set; }

    public IReadOnlyCollection<CurriculumSubjectPrerequisite> Prerequisites => _prerequisites.AsReadOnly();

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

    public CurriculumSubject UpdateYearLevel(int newYearLevel)
    {
        if (YearLevel == newYearLevel) return this;
        YearLevel = Guard.Against.OutOfRange(newYearLevel, nameof(newYearLevel), 1, 6);
        return this;
    }

    public CurriculumSubject UpdateSemester(int newSemester)
    {
        if (Semester == newSemester) return this;
        Semester = Guard.Against.OutOfRange(newSemester, nameof(newSemester), 1, 3);
        return this;
    }

    public CurriculumSubject UpdateElectiveInfo(bool isElective, string? electiveGroupName)
    {
        IsElective = isElective;
        ElectiveGroupName = electiveGroupName;
        return this;
    }

    public CurriculumSubject AddPrerequisite(
        CurriculumSubjectId prerequisiteCurriculumSubjectId,
        decimal? minimumGrade,
        UserId addedBy)
    {
        Guard.Against.Null(prerequisiteCurriculumSubjectId);
        Guard.Against.Null(addedBy);

        // Prevent self-reference
        if (Id == prerequisiteCurriculumSubjectId)
            throw new ArgumentException("A subject cannot be its own prerequisite.");

        // Prevent duplicate prerequisites
        if (_prerequisites.Any(p => p.PrerequisiteCurriculumSubjectId == prerequisiteCurriculumSubjectId && p.IsActive))
            return this;

        var prerequisite = new CurriculumSubjectPrerequisite(
            Id,
            prerequisiteCurriculumSubjectId,
            minimumGrade,
            addedBy);

        _prerequisites.Add(prerequisite);
        return this;
    }

    public CurriculumSubject RemovePrerequisite(CurriculumSubjectId prerequisiteCurriculumSubjectId)
    {
        var prerequisite = _prerequisites.FirstOrDefault(p =>
            p.PrerequisiteCurriculumSubjectId == prerequisiteCurriculumSubjectId);

        if (prerequisite != null)
        {
            _prerequisites.Remove(prerequisite);
        }

        return this;
    }

    public IEnumerable<CurriculumSubjectPrerequisite> GetActivePrerequisites()
    {
        return _prerequisites.Where(p => p.IsActive);
    }
}
