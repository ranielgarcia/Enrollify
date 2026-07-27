using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate;

public class ClassSection : EntityBase<ClassSection, ClassSectionId>, IAggregateRoot, IAuditable
{
    private ClassSection()
    {
    }

    public ClassSection(ClassSectionForCreation sectionForCreation)
    {
        Name = Guard.Against.NullOrEmpty(sectionForCreation.Name, nameof(sectionForCreation.Name));
        IntendedYearLevel = Guard.Against.Null(sectionForCreation.IntendedYearLevel,
            nameof(sectionForCreation.IntendedYearLevel));
        CourseId = Guard.Against.Null(sectionForCreation.CourseId, nameof(sectionForCreation.CourseId));
        CurriculumId = Guard.Against.Null(sectionForCreation.CurriculumId, nameof(sectionForCreation.CurriculumId));
        AcademicTermId =
            Guard.Against.Null(sectionForCreation.AcademicTermId, nameof(sectionForCreation.AcademicTermId));
        CohortAcademicYearId = Guard.Against.Null(sectionForCreation.CohortAcademicYearId,
            nameof(sectionForCreation.CohortAcademicYearId));
        AdviserId = sectionForCreation.AdviserId;
        SectionCode = Guard.Against.Null(sectionForCreation.SectionCode, nameof(sectionForCreation.SectionCode));
        StatusId = sectionForCreation.InitializeStatus;
    }

    public string Name { get; private set; } = null!;
    public YearLevel IntendedYearLevel { get; private set; }

    public CourseId CourseId { get; private set; }
    public Course? Course { get; private set; }

    public CurriculumId CurriculumId { get; private set; }
    public Curriculum? Curriculum { get; private set; }

    public AcademicTermId AcademicTermId { get; private set; }
    public AcademicTerm? AcademicTerm { get; private set; }

    public AcademicYearId CohortAcademicYearId { get; private set; }
    public AcademicYear? CohortAcademicYear { get; private set; }

    public TeacherId? AdviserId { get; private set; }
    public Teacher? Adviser { get; private set; }

    public SectionCode SectionCode { get; private set; }

    public ClassSectionStatusEnum StatusId { get; private set; }

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

    public string FullName => $"{Name}-{IntendedYearLevel.Value.ToString()}{SectionCode.Value.ToString()}";

    public ClassSection UpdateSectionCode(SectionCode newSectionCode)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section name can only be updated in Draft status.");
        if (newSectionCode == SectionCode) return this;
        SectionCode = Guard.Against.Null(newSectionCode, nameof(newSectionCode));
        return this;
    }

    public ClassSection UpdateName(string newName)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section name can only be updated in Draft status.");
        if (newName == Name) return this;
        Name = Guard.Against.NullOrEmpty(newName, nameof(newName));
        return this;
    }

    public ClassSection UpdateYearLevel(YearLevel newIntendedYearLevel)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section year level can only be updated in Draft status.");
        if (newIntendedYearLevel == IntendedYearLevel) return this;
        IntendedYearLevel = Guard.Against.Null(newIntendedYearLevel, nameof(newIntendedYearLevel));
        return this;
    }

    public ClassSection UpdateCourse(CourseId newCourseId)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section course can only be updated in Draft status.");
        if (newCourseId == CourseId) return this;
        CourseId = Guard.Against.Null(newCourseId, nameof(newCourseId));
        return this;
    }

    public ClassSection UpdateCurriculum(CurriculumId newCurriculumId)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section curriculum can only be updated in Draft status.");
        if (newCurriculumId == CurriculumId) return this;
        CurriculumId = Guard.Against.Null(newCurriculumId, nameof(newCurriculumId));
        return this;
    }

    public ClassSection UpdateAcademicTerm(AcademicTermId newAcademicTermId)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section academic term can only be updated in Draft status.");
        if (newAcademicTermId == AcademicTermId) return this;
        AcademicTermId = Guard.Against.Null(newAcademicTermId, nameof(newAcademicTermId));
        return this;
    }

    public ClassSection UpdateCohortAcademicYearId(AcademicYearId newCohortAcademicYearId)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section cohort academic year can only be updated in Draft status.");
        if (newCohortAcademicYearId == CohortAcademicYearId) return this;
        CohortAcademicYearId = Guard.Against.Null(newCohortAcademicYearId, nameof(newCohortAcademicYearId));
        return this;
    }

    public ClassSection UpdateAdviser(TeacherId newAdviserId)
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft || s == ClassSectionStatusEnum.Open,
            "Section adviser can only be updated in Draft or Open status.");
        if (newAdviserId == AdviserId) return this;
        AdviserId = Guard.Against.Null(newAdviserId, nameof(newAdviserId));
        return this;
    }

    public ClassSection MoveToDraft()
    {
      StatusId = ClassSectionStatusEnum.Draft;
      RegisterDomainEvent(new ClassSectionMovedToDraftEvent(Id));
      return this;
    }

    public ClassSection MoveToValidating()
    {
      StatusId = ClassSectionStatusEnum.Validating;
      RegisterDomainEvent(new ClassSectionMovedToValidatingEvent(Id));
      return this;
    }

    public ClassSection OpenForEnrollment()
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft,
            "Section must be in Draft status to open for enrollment.");
        StatusId = ClassSectionStatusEnum.Open;
        RegisterDomainEvent(new ClassSectionOpenedForEnrollmentEvent(Id));
        return this;
    }

    public ClassSection LockEnrollment()
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Open,
            "Section must be in Open status to lock enrollment.");
        StatusId = ClassSectionStatusEnum.Locked;
        RegisterDomainEvent(new ClassSectionEnrollmentLockedEvent(Id));
        return this;
    }

    public ClassSection Activate()
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Locked,
            "Section must be in Locked status to activate.");
        StatusId = ClassSectionStatusEnum.Active;
        RegisterDomainEvent(new ClassSectionActivatedEvent(Id));
        return this;
    }

    public ClassSection Complete()
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Active,
            "Section must be in Active status to complete.");
        StatusId = ClassSectionStatusEnum.Completed;
        RegisterDomainEvent(new ClassSectionCompletedEvent(Id));
        return this;
    }

    public ClassSection Cancel()
    {
        Guard.Against.InvalidInput(StatusId, nameof(StatusId),
            s => s == ClassSectionStatusEnum.Draft
                 || s == ClassSectionStatusEnum.Open
                 || s == ClassSectionStatusEnum.Locked,
            "Section can only be cancelled from Draft, Open, or Locked status.");
        StatusId = ClassSectionStatusEnum.Cancelled;
        RegisterDomainEvent(new ClassSectionCancelledEvent(Id));
        return this;
    }
}
