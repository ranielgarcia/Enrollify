using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate;

public class ClassSection : EntityBase<ClassSection, ClassSectionId>, IAggregateRoot, IAuditable
{
    private ClassSection() {}

    public ClassSection(ClassSectionForCreation sectionForCreation)
    {
        Name = Guard.Against.NullOrEmpty(sectionForCreation.Name, nameof(sectionForCreation.Name));
        YearLevel = Guard.Against.Null(sectionForCreation.YearLevel, nameof(sectionForCreation.YearLevel));
        CourseId = Guard.Against.Null(sectionForCreation.CourseId, nameof(sectionForCreation.CourseId));
        AcademicTermId = Guard.Against.Null(sectionForCreation.AcademicTermId, nameof(sectionForCreation.AcademicTermId));
        AdviserId = Guard.Against.Null(sectionForCreation.AdviserId, nameof(sectionForCreation.AdviserId));
        SectionCode = Guard.Against.Null(sectionForCreation.SectionCode, nameof(sectionForCreation.SectionCode));
    }

    public string Name { get; private set; } = null!;
    public YearLevel YearLevel { get; private set; }

    public CourseId CourseId { get; private set; }
    public Course? Course { get; private set; }

    public AcademicTermId AcademicTermId { get; private set; }
    public AcademicTerm? AcademicTerm { get; private set; }

    public TeacherId AdviserId { get; private set; }
    public Teacher? Adviser { get; private set; }

    public SectionCode SectionCode { get; private set; }


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

    public ClassSection UpdateName (string newName)
    {
        if (newName == Name) return this;
        Name = Guard.Against.NullOrEmpty(newName, nameof(newName));
        return this;
    }


    public ClassSection UpdateYearLevel(YearLevel newYearLevel)
    {
        if (newYearLevel == YearLevel) return this;
        YearLevel = Guard.Against.Null(newYearLevel, nameof(newYearLevel));
        return this;
    }

    public ClassSection UpdateCourse(CourseId newCourseId)
    {
        if (newCourseId == CourseId) return this;
        CourseId = Guard.Against.Null(newCourseId, nameof(newCourseId));
        return this;
    }

    public ClassSection UpdateAcademicTerm(AcademicTermId newAcademicTermId)
    {
        if (newAcademicTermId == AcademicTermId) return this;
        AcademicTermId = Guard.Against.Null(newAcademicTermId, nameof(newAcademicTermId));
        return this;
    }

    public ClassSection UpdateAdviser(TeacherId newAdviserId)
    {
        if (newAdviserId == AdviserId) return this;
        AdviserId = Guard.Against.Null(newAdviserId, nameof(newAdviserId));
        return this;
    }
}
