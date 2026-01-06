using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Aggregates.CurriculaAggregate;

/// <summary>
/// Represents a prerequisite relationship between curriculum subjects.
/// Prerequisites are scoped to the curriculum level, allowing the same subject 
/// to have different prerequisites in different curriculum versions.
/// </summary>
public class CurriculumSubjectPrerequisite : IAuditable
{
    private CurriculumSubjectPrerequisite() { } // EF Core constructor

    public CurriculumSubjectPrerequisite(
        CurriculumSubjectId curriculumSubjectId,
        CurriculumSubjectId prerequisiteCurriculumSubjectId,
        decimal? minimumGrade,
        UserId addedBy)
    {
        CurriculumSubjectId = curriculumSubjectId;
        PrerequisiteCurriculumSubjectId = prerequisiteCurriculumSubjectId;
        MinimumGrade = minimumGrade;
        CreatedBy = addedBy;
    }

    public CurriculumSubjectId CurriculumSubjectId { get; private set; }
    public CurriculumSubjectId PrerequisiteCurriculumSubjectId { get; private set; }

    /// <summary>
    /// Optional minimum grade required (e.g., 2.0). Valid range: 1.0 to 5.0
    /// </summary>
    public decimal? MinimumGrade { get; private set; }

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

    public CurriculumSubjectPrerequisite UpdateMinimumGrade(decimal? newMinimumGrade)
    {
        if (MinimumGrade == newMinimumGrade) return this;
        MinimumGrade = newMinimumGrade;
        return this;
    }
}
