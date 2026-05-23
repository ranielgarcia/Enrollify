using System.Drawing;
using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

// DO NOT ALLOW ANY UPDATES
//CourseCurriculumAssignments: Locks a curriculum to a cohort.
//A cohort is identified by Course + Entry Academic Year (the AY when Year 1 students first enroll).
//When creating class sections for Year N in AY X:
//  EntryAcademicYear = the AcademicYear where StartDate.Year = X.StartDate.Year - (N - 1)
//  Then look up(CourseId, EntryAcademicYearId) → CurriculumId from this table.
public class CourseCurriculumAssignment : EntityBase<CourseCurriculumAssignment, CourseCurriculumAssignmentId>, IAggregateRoot, IAuditable
{
    private CourseCurriculumAssignment(){}

    public CourseCurriculumAssignment(CourseId courseId, AcademicYearId entryAcademicYearId, CurriculumId curriculumId)
    {
        CourseId = Guard.Against.Null(courseId, nameof(courseId));
        EntryAcademicYearId = Guard.Against.Null(entryAcademicYearId, nameof(entryAcademicYearId));
        CurriculumId = Guard.Against.Null(curriculumId, nameof(curriculumId));
    }

    public CourseId CourseId { get; private set; }
    public Course? Course { get; private set; }

    public AcademicYearId EntryAcademicYearId { get; private set; }
    public AcademicYear? EntryAcademicYear { get; private set; }

    public CurriculumId CurriculumId { get; private set; }
    public Curriculum? Curriculum { get; private set; }


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

    public CourseCurriculumAssignment UpdateCurriculum(CurriculumId newCurriculumId)
    {
        if (CurriculumId == newCurriculumId) return this;
        CurriculumId = Guard.Against.Null(newCurriculumId, nameof(newCurriculumId));
        return this;
    }

}
