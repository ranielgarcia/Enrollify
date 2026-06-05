using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate.Models;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.Services.ClassScheduleValidation;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

public class ClassSectionSubjectOffering : EntityBase<ClassSectionSubjectOffering, ClassSectionSubjectOfferingId>,
  IAggregateRoot, IAuditable
{
  private readonly List<ClassSchedule> _classSchedules = new();

  private ClassSectionSubjectOffering()
  {
  }

  public ClassSectionSubjectOffering(ClassSectionSubjectOfferingForCreation forCreation)
  {
    SubjectId = Guard.Against.Null(forCreation.SubjectId, nameof(forCreation.SubjectId));
    TeacherId = forCreation.TeacherId;
    ClassSectionId = Guard.Against.Null(forCreation.ClassSectionId, nameof(forCreation.ClassSectionId));
    RoomId = forCreation.RoomId; // RoomId is optional - no guard check needed
    DaysPerWeek = Guard.Against.NegativeOrZero(forCreation.DaysPerWeek, nameof(forCreation.DaysPerWeek));
    HoursPerDay = Guard.Against.NegativeOrZero(forCreation.HoursPerDay, nameof(forCreation.HoursPerDay));
    MaxNumberOfStudents = forCreation.MaxNumberOfStudents;
    CurriculumSubjectId =
      Guard.Against.Null(forCreation.CurriculumSubjectId, nameof(forCreation.CurriculumSubjectId));
    SnapshotSubjectCode =
      Guard.Against.Null(forCreation.SnapshotSubjectCode, nameof(forCreation.SnapshotSubjectCode));
    SnapshotSubjectTitle =
      Guard.Against.NullOrEmpty(forCreation.SnapshotSubjectTitle, nameof(forCreation.SnapshotSubjectTitle));
    SnapshotUnits = Guard.Against.NegativeOrZero(forCreation.SnapshotUnits, nameof(forCreation.SnapshotUnits));
    SnapshotIsElective = Guard.Against.Null(forCreation.SnapshotIsElective, nameof(forCreation.SnapshotIsElective));
    SnapshotElectiveGroupName = forCreation.SnapshotElectiveGroupName;
  }

  public ClassSectionId ClassSectionId { get; private set; }

  public SubjectId SubjectId { get; private set; }
  public Subject? Subject { get; private set; }

  public TeacherId? TeacherId { get; private set; }
  public Teacher? Teacher { get; private set; }


  public RoomId? RoomId { get; private set; }
  public Room? Room { get; private set; }

  public int DaysPerWeek { get; private set; }
  public decimal HoursPerDay { get; private set; }
  public int? MaxNumberOfStudents { get; private set; }


  public CurriculumSubjectId CurriculumSubjectId { get; private set; }
  public SubjectCode SnapshotSubjectCode { get; private set; }
  public string SnapshotSubjectTitle { get; private set; }
  public decimal SnapshotUnits { get; private set; }
  public bool SnapshotIsElective { get; private set; }
  public string? SnapshotElectiveGroupName { get; private set; }

  public IReadOnlyCollection<ClassSchedule> ClassSchedules => _classSchedules.AsReadOnly();

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

  public ClassSectionSubjectOffering UpdateTeacher(TeacherId newTeacherId)
  {
    if (newTeacherId == TeacherId) return this;
    TeacherId = Guard.Against.Null(newTeacherId, nameof(newTeacherId));
    return this;
  }

  public ClassSectionSubjectOffering UpdateRoom(RoomId newRoomId)
  {
    if (newRoomId == RoomId) return this;
    RoomId = Guard.Against.Null(newRoomId, nameof(newRoomId));
    return this;
  }

  public ClassSectionSubjectOffering UpdateSchedule(int newDaysPerWeek, decimal newHoursPerDay)
  {
    if (newDaysPerWeek == DaysPerWeek && newHoursPerDay == HoursPerDay) return this;
    DaysPerWeek = Guard.Against.NegativeOrZero(newDaysPerWeek, nameof(newDaysPerWeek));
    HoursPerDay = Guard.Against.NegativeOrZero(newHoursPerDay, nameof(newHoursPerDay));
    return this;
  }

  public ClassSectionSubjectOffering UpdateMaxNumberOfStudents(int? maxNumberOfStudents)
  {
    if (maxNumberOfStudents == MaxNumberOfStudents) return this;
    MaxNumberOfStudents = maxNumberOfStudents;
    return this;
  }

  public ClassSectionSubjectOffering UpdateCurriculumSubjectId(CurriculumSubjectId curriculumSubjectId)
  {
    if (curriculumSubjectId == CurriculumSubjectId) return this;
    CurriculumSubjectId = Guard.Against.Null(curriculumSubjectId, nameof(curriculumSubjectId));
    return this;
  }

  public ClassSectionSubjectOffering UpdateSnapshotSubjectCode(SubjectCode snapshotSubjectCode)
  {
    if (snapshotSubjectCode == SnapshotSubjectCode) return this;
    SnapshotSubjectCode = Guard.Against.Null(snapshotSubjectCode, nameof(snapshotSubjectCode));
    return this;
  }

  public ClassSectionSubjectOffering UpdateSnapshotSubjectTitle(string snapshotSubjectTitle)
  {
    if (snapshotSubjectTitle == SnapshotSubjectTitle) return this;
    SnapshotSubjectTitle = Guard.Against.NullOrWhiteSpace(snapshotSubjectTitle, nameof(snapshotSubjectTitle));
    return this;
  }

  public ClassSectionSubjectOffering UpdateSnapshotUnits(decimal snapshotUnits)
  {
    if (snapshotUnits == SnapshotUnits) return this;
    SnapshotUnits = snapshotUnits;
    return this;
  }

  public ClassSectionSubjectOffering UpdateSnapshotIsElective(bool snapshotIsElective)
  {
    if (snapshotIsElective == SnapshotIsElective) return this;
    SnapshotIsElective = snapshotIsElective;
    return this;
  }

  public ClassSectionSubjectOffering UpdateSnapshotElectiveGroupName(string snapshotElectiveGroupName)
  {
    if (snapshotElectiveGroupName == SnapshotElectiveGroupName) return this;
    SnapshotElectiveGroupName = Guard.Against.Null(snapshotElectiveGroupName, nameof(snapshotElectiveGroupName));
    return this;
  }

  public bool IsFullyScheduled()
  {
    return _classSchedules.Count == DaysPerWeek;
  }

  public ClassSectionSubjectOffering AddClassSchedule(ClassSchedule classSchedule)
  {
    Guard.Against.Null(classSchedule, nameof(classSchedule));

    ClassScheduleValidationResult validationResult = ClassScheduleValidationService.ValidateAddClassSchedule(
      _classSchedules,
      classSchedule,
      DaysPerWeek,
      HoursPerDay);

    if (!validationResult.IsValid)
      throw new InvalidClassScheduleException(validationResult.ErrorMessage);

    _classSchedules.Add(classSchedule);
    return this;
  }

  public ClassSectionSubjectOffering UpdateClassSchedule(ClassScheduleId classScheduleId, TimeOnly newStartTime,
    TimeOnly newEndTime)
  {
    Guard.Against.Null(classScheduleId, nameof(classScheduleId));
    Guard.Against.Default(newStartTime, nameof(newStartTime));
    Guard.Against.Default(newEndTime, nameof(newEndTime));

    ClassSchedule? existingSchedule = _classSchedules.FirstOrDefault(cs => cs.Id == classScheduleId);
    if (existingSchedule is null)
      throw new InvalidClassScheduleException($"Class schedule with ID {classScheduleId.Value} not found.");

    if (existingSchedule.StartTime == newStartTime && existingSchedule.EndTime == newEndTime) return this;

    Guard.Against.InvalidInput(newEndTime, nameof(newEndTime), e => e > newStartTime,
      "End time must be after start time.");

    ClassScheduleValidationResult validationResult = ClassScheduleValidationService.ValidateUpdateClassSchedule(
      _classSchedules,
      classScheduleId,
      newStartTime,
      newEndTime,
      DaysPerWeek,
      HoursPerDay);

    if (!validationResult.IsValid)
      throw new InvalidClassScheduleException(validationResult.ErrorMessage);

    _classSchedules.Remove(existingSchedule);
    var updatedSchedule = new ClassSchedule(
      Id,
      existingSchedule.DayOfWeek,
      newStartTime,
      newEndTime
    );
    _classSchedules.Add(updatedSchedule);
    return this;
  }

  public ClassSectionSubjectOffering RemoveClassSchedule(ClassScheduleId classScheduleId)
  {
    Guard.Against.Null(classScheduleId, nameof(classScheduleId));

    ClassSchedule? existingSchedule = _classSchedules.FirstOrDefault(cs => cs.Id == classScheduleId);
    if (existingSchedule is null)
      throw new InvalidClassScheduleException($"Class schedule with ID {classScheduleId.Value} not found.");

    _classSchedules.Remove(existingSchedule);
    return this;
  }
}
