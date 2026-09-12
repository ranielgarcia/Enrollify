using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

public class ClassSectionValidationIssue : EntityBase<ClassSectionValidationIssue, ClassSectionValidationIssueId>,
  IAggregateRoot
{
  private ClassSectionValidationIssue()
  {
  }

  // Constructor for conflict-origin issues (has times, offerings, affected offerings)
  public ClassSectionValidationIssue(
    CourseId courseId,
    AcademicTermId academicTermId,
    ClassSectionId classSectionId,
    ClassScheduleConflictResult classScheduleConflictResult)
  {
    CourseId = Guard.Against.Null(courseId);
    AcademicTermId = Guard.Against.Null(academicTermId);
    ClassSectionId = Guard.Against.Null(classSectionId);
    OfferingId = Guard.Against.Null(classScheduleConflictResult.OfferingId);
    Type = Guard.Against.Null(classScheduleConflictResult.Type);
    Message = Guard.Against.NullOrEmpty(classScheduleConflictResult.Message);
    DayOfWeek = Guard.Against.Null(classScheduleConflictResult.DayOfWeek);
    StartTime = classScheduleConflictResult.StartTime;
    EndTime = classScheduleConflictResult.EndTime;
    ConflictingOfferings = classScheduleConflictResult.ConflictingOfferings ?? [];
    ComputedAt = DateTimeOffset.UtcNow;
  }

  // Constructor for data integrity issues (may lack offering/time)
  public ClassSectionValidationIssue(
    CourseId courseId,
    AcademicTermId academicTermId,
    ClassSectionId classSectionId,
    ClassSectionDataIntegrityResult dataIntegrityResult)
  {
    CourseId = Guard.Against.Null(courseId);
    AcademicTermId = Guard.Against.Null(academicTermId);
    ClassSectionId = Guard.Against.Null(classSectionId);
    OfferingId = dataIntegrityResult.OfferingId; // nullable
    Type = Guard.Against.Null(dataIntegrityResult.Type);
    Message = Guard.Against.NullOrEmpty(dataIntegrityResult.Message);
    DayOfWeek = null;
    StartTime = null;
    EndTime = null;
    ConflictingOfferings = [];
    ComputedAt = DateTimeOffset.UtcNow;
  }

  public CourseId CourseId { get; private set; }

  public AcademicTermId AcademicTermId { get; private set; }

  public ClassSectionId ClassSectionId { get; private set; }
  public ClassSectionSubjectOfferingId? OfferingId { get; private set; }

  public ClassSectionValidationIssueTypeEnum Type { get; private set; } =
    ClassSectionValidationIssueTypeEnum.NO_OFFERINGS;

  public string Message { get; private set; } = null!;
  public DayOfWeekEnum? DayOfWeek { get; private set; }

  public TimeOnly? StartTime { get; private set; }
  public TimeOnly? EndTime { get; private set; }

  public DateTimeOffset ComputedAt { get; private set; }

  public List<ClassScheduleConflictingOffering> ConflictingOfferings { get; private set; } = [];
}
