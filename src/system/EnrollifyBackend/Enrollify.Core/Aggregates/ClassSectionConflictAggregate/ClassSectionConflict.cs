using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionConflictAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionConflictAggregate;

public class ClassSectionConflict : EntityBase<ClassSectionConflict, ClassSectionConflictId>, IAggregateRoot
{
  private ClassSectionConflict() { }

  public ClassSectionConflict(
    CollegeId collegeId,
    CourseId courseId,
    AcademicTermId academicTermId,
    ClassSectionId classSectionId,
    ClassSectionSubjectOfferingId offeringId,
    ConflictResult conflictResult
    )
  {
    CollegeId = Guard.Against.Null(collegeId, nameof(collegeId));
    CourseId = Guard.Against.Null(courseId, nameof(courseId));
    AcademicTermId = Guard.Against.Null(academicTermId, nameof(academicTermId));
    ClassSectionId = Guard.Against.Null(classSectionId, nameof(classSectionId));
    OfferingId = Guard.Against.Null(offeringId, nameof(offeringId));
    ConflictType = Guard.Against.Null(conflictResult.Type, nameof(conflictResult.Type));
    Severity = Guard.Against.Null(conflictResult.Severity, nameof(conflictResult.Severity));
    Message = Guard.Against.NullOrEmpty(conflictResult.Message, nameof(conflictResult.Message));
    DayOfWeek = Guard.Against.Null(conflictResult.DayOfWeek, nameof(conflictResult.DayOfWeek));
    StartTime = conflictResult.StartTime;
    EndTime = conflictResult.EndTime;
    AffectedOfferings = conflictResult.AffectedOfferings;
  }

  public CollegeId CollegeId { get; private set; }

  public CourseId CourseId { get; private set; }

  public AcademicTermId AcademicTermId { get; private set; }

  public ClassSectionId ClassSectionId { get; private set; }
  public ClassSectionSubjectOfferingId OfferingId { get; private set; }

  public ClassScheduleConflictTypeEnum ConflictType { get; private set; }

  public DomainValidationErrorSeverityEnum Severity { get; private set; }

  public string Message { get; private set; } = null!;
  public DayOfWeekEnum DayOfWeek { get; private set; }

  public TimeOnly? StartTime { get; private set; }
  public TimeOnly? EndTime { get; private set; }

  public DateTimeOffset ComputedAt { get; private set; }

  public List<AffectedOffering> AffectedOfferings { get; private set; }
}
