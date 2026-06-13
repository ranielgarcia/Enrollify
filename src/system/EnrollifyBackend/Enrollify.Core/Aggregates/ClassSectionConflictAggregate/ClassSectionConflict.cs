using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionConflictAggregate;

public class ClassSectionConflict : EntityBase<ClassSectionConflict, ClassSectionConflictId>, IAggregateRoot
{
  private readonly List<ClassSectionConflictAffectedOffering> _affectedOfferings = new();

  private ClassSectionConflict() { }

  public ClassSectionConflict(
    CollegeId collegeId,
    CourseId courseId,
    AcademicTermId academicTermId,
    ClassSectionId classSectionId,
    ClassSectionSubjectOfferingId offeringId,
    ClassScheduleConflictTypeEnum conflictType,
    DomainValidationErrorSeverityEnum severity,
    string message,
    DayOfWeekEnum dayOfWeek,
    TimeOnly? startTime,
    TimeOnly? endTime,
    DateTimeOffset computedAt,
    IEnumerable<ClassSectionSubjectOfferingId>? affectedOfferingIds
    )
  {
    CollegeId = Guard.Against.Null(collegeId, nameof(collegeId));
    CourseId = Guard.Against.Null(courseId, nameof(courseId));
    AcademicTermId = Guard.Against.Null(academicTermId, nameof(academicTermId));
    ClassSectionId = Guard.Against.Null(classSectionId, nameof(classSectionId));
    OfferingId = Guard.Against.Null(offeringId, nameof(offeringId));
    ConflictType = Guard.Against.Null(conflictType, nameof(conflictType));
    Severity = Guard.Against.Null(severity, nameof(severity));
    Message = Guard.Against.NullOrEmpty(message, nameof(message));
    DayOfWeek = Guard.Against.Null(dayOfWeek, nameof(dayOfWeek));
    StartTime = startTime;
    EndTime = endTime;
    ComputedAt = Guard.Against.Null(computedAt, nameof(computedAt));

    if (affectedOfferingIds != null && affectedOfferingIds.Any())
    {
      _affectedOfferings.AddRange(affectedOfferingIds.Select(offeringId => new ClassSectionConflictAffectedOffering(Id, offeringId)));
    }
  }

  public IReadOnlyCollection<ClassSectionConflictAffectedOffering> AffectedOfferings => _affectedOfferings.AsReadOnly();

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

}
