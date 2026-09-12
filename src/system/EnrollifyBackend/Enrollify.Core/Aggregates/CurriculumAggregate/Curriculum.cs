using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CurriculumAggregate;

/// <summary>
/// Represents a curriculum version for a course.
/// Each course can have multiple curriculum versions.
/// Students are assigned to a curriculum when they enroll.
/// Prerequisites are defined at the curriculum level, not the subject level.
/// </summary>
public class Curriculum : EntityBase<Curriculum, CurriculumId>, IAggregateRoot, IAuditable
{
    private readonly List<CurriculumSubject> _curriculumSubjects = new();

    private Curriculum() { } // EF Core constructor

    public static Curriculum CreateDraftCurriculum(DraftCurriculumForCreation newCurricula)
    {
        return new Curriculum
        {
            CourseId = Guard.Against.Null(newCurricula.CourseId),
            EffectiveYear = Year.From(Guard.Against.OutOfRange(newCurricula.EffectiveYear.Value, nameof(newCurricula.EffectiveYear), 2000, 9999)),
            Version = Guard.Against.NullOrWhiteSpace(newCurricula.Version),
            StatusId = CurriculumStatusEnum.Draft,
            Description = newCurricula.Description,
        };
    }

    public CourseId CourseId { get; private set; }

    /// <summary>
    /// Academic year when this curriculum takes effect (e.g., 2024)
    /// </summary>
    public Year EffectiveYear { get; private set; }

    /// <summary>
    /// Version identifier (e.g., '2024-A', '2024-REV1')
    /// </summary>
    public string Version { get; private set; } = string.Empty;

    public CurriculumStatusEnum StatusId { get; private set; } = CurriculumStatusEnum.Draft;
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

    public Curriculum UpdateCourse (CourseId newCourseId)
    {
        if (CourseId == newCourseId) return this;
        CourseId = Guard.Against.Null(newCourseId);
        return this;
    }

    public Curriculum UpdateEffectiveYear(int newEffectiveYear)
    {
        if (EffectiveYear == newEffectiveYear) return this;
        EffectiveYear = Year.From(Guard.Against.OutOfRange(newEffectiveYear, nameof(newEffectiveYear), 2000, 9999));
        return this;
    }

    public Curriculum UpdateVersion(string newVersion)
    {
        if (Version == newVersion) return this;
        Version = Guard.Against.NullOrWhiteSpace(newVersion);
        return this;
    }

    public Curriculum UpdateStatus(CurriculumStatusEnum newStatus)
    {
        if (StatusId == newStatus) return this;
        StatusId = Guard.Against.Null(newStatus);
        return this;
    }

    public Curriculum UpdateDescription(string? newDescription)
    {
        if (Description == newDescription) return this;
        Description = newDescription;
        return this;
    }

    public Curriculum Approve(DateTimeOffset approvedDate)
    {
        ApprovedDate = approvedDate;
        StatusId = CurriculumStatusEnum.Active;
        return this;
    }

    public CurriculumSubject? AddSubject(
      SubjectId subjectId,
      int yearLevel,
      int term,
      bool isElective,
      string? electiveGroupName,
      decimal? subjectUnitsOverride,
      int daysPerWeek = 1,
      decimal hoursPerDay = 1)
    {
        Guard.Against.Null(subjectId, message: "Subject ID is required");

        // Prevent duplicate subjects in the same curriculum
        if (_curriculumSubjects.Any(cs => cs.SubjectId == subjectId && cs.IsActive))
            return null;

        yearLevel = Guard.Against.OutOfRange(yearLevel, nameof(yearLevel), 1, 6);
        term = Guard.Against.OutOfRange(term, nameof(term), 1, 3);

        var curriculumSubject = new CurriculumSubject(
            Id,
            subjectId,
            YearLevel.From(yearLevel),
            TermNumber.From(term),
            isElective,
            electiveGroupName,
            subjectUnitsOverride,
            daysPerWeek,
            hoursPerDay);

        _curriculumSubjects.Add(curriculumSubject);
        return curriculumSubject;
    }

    public Curriculum RemoveSubject(SubjectId subjectId)
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

    public IEnumerable<CurriculumSubject> GetSubjectsByYearAndTerm(YearLevel yearLevel, TermNumber termNumber)
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
