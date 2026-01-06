using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.CurriculaAggregate.Models;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CurriculaAggregate;

/// <summary>
/// Represents a curriculum version for a course.
/// Each course can have multiple curriculum versions.
/// Students are assigned to a curriculum when they enroll.
/// Prerequisites are defined at the curriculum level, not the subject level.
/// </summary>
public class Curricula : EntityBase<Curricula, CurriculaId>, IAggregateRoot, IAuditable
{
    private readonly List<CurriculumSubject> _curriculumSubjects = new();

    private Curricula() { } // EF Core constructor

    public Curricula(CurriculaForCreation newCurricula)
    {
        CourseId = Guard.Against.Null(newCurricula.CourseId);
        EffectiveYear = Guard.Against.OutOfRange(newCurricula.EffectiveYear, nameof(newCurricula.EffectiveYear), 2000, 9999);
        Version = Guard.Against.NullOrWhiteSpace(newCurricula.Version);
        StatusId = Guard.Against.Null(newCurricula.StatusId);
        Description = newCurricula.Description;
        ApprovedDate = newCurricula.ApprovedDate;
    }

    public CourseId CourseId { get; private set; }

    /// <summary>
    /// Academic year when this curriculum takes effect (e.g., 2024)
    /// </summary>
    public int EffectiveYear { get; private set; }

    /// <summary>
    /// Version identifier (e.g., '2024-A', '2024-REV1')
    /// </summary>
    public string Version { get; private set; }

    public CurriculaStatusEnum StatusId { get; private set; }
    public string? Description { get; private set; }

    /// <summary>
    /// When the curriculum was officially approved
    /// </summary>
    public DateTimeOffset? ApprovedDate { get; private set; }

    public Course? Course { get; private set; }

    public IReadOnlyCollection<CurriculumSubject> CurriculumSubjects => _curriculumSubjects.AsReadOnly();

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

    public Curricula UpdateEffectiveYear(int newEffectiveYear)
    {
        if (EffectiveYear == newEffectiveYear) return this;
        EffectiveYear = Guard.Against.OutOfRange(newEffectiveYear, nameof(newEffectiveYear), 2000, 9999);
        return this;
    }

    public Curricula UpdateVersion(string newVersion)
    {
        if (Version == newVersion) return this;
        Version = Guard.Against.NullOrWhiteSpace(newVersion);
        return this;
    }

    public Curricula UpdateStatus(CurriculaStatusEnum newStatus)
    {
        if (StatusId == newStatus) return this;
        StatusId = Guard.Against.Null(newStatus);
        return this;
    }

    public Curricula UpdateDescription(string? newDescription)
    {
        if (Description == newDescription) return this;
        Description = newDescription;
        return this;
    }

    public Curricula Approve(DateTimeOffset approvedDate)
    {
        ApprovedDate = approvedDate;
        StatusId = CurriculaStatusEnum.Active;
        return this;
    }

    public Curricula AddSubject(
        SubjectId subjectId,
        int yearLevel,
        int semester,
        bool isElective,
        string? electiveGroupName,
        UserId addedBy)
    {
        Guard.Against.Null(subjectId);
        Guard.Against.Null(addedBy);

        // Prevent duplicate subjects in the same curriculum
        if (_curriculumSubjects.Any(cs => cs.SubjectId == subjectId && cs.IsActive))
            return this;

        yearLevel = Guard.Against.OutOfRange(yearLevel, nameof(yearLevel), 1, 6);
        semester = Guard.Against.OutOfRange(semester, nameof(semester), 1, 3);

        var curriculumSubject = new CurriculumSubject(
            Id,
            subjectId,
            yearLevel,
            semester,
            isElective,
            electiveGroupName,
            addedBy);

        _curriculumSubjects.Add(curriculumSubject);
        return this;
    }

    public Curricula RemoveSubject(SubjectId subjectId)
    {
        var curriculumSubject = _curriculumSubjects.FirstOrDefault(cs => cs.SubjectId == subjectId);
        if (curriculumSubject != null)
        {
            _curriculumSubjects.Remove(curriculumSubject);
        }
        return this;
    }

    public CurriculumSubject? GetCurriculumSubject(SubjectId subjectId)
    {
        return _curriculumSubjects.FirstOrDefault(cs => cs.SubjectId == subjectId && cs.IsActive);
    }

    public CurriculumSubject? GetCurriculumSubjectById(CurriculumSubjectId curriculumSubjectId)
    {
        return _curriculumSubjects.FirstOrDefault(cs => cs.Id == curriculumSubjectId && cs.IsActive);
    }

    public IEnumerable<CurriculumSubject> GetSubjectsByYearAndSemester(int yearLevel, int termNumber)
    {
        return _curriculumSubjects.Where(cs =>
            cs.YearLevel == yearLevel &&
            cs.TermNumber == termNumber &&
            cs.IsActive);
    }

    public IEnumerable<CurriculumSubject> GetElectives()
    {
        return _curriculumSubjects.Where(cs => cs.IsElective && cs.IsActive);
    }

    public IEnumerable<CurriculumSubject> GetActiveSubjects()
    {
        return _curriculumSubjects.Where(cs => cs.IsActive);
    }
}
