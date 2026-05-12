using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

public class ClassSectionSubjectOffering : EntityBase<ClassSectionSubjectOffering, ClassSectionSubjectOfferingId>, IAggregateRoot, IAuditable
{
    private readonly List<ClassSchedule> _classSchedules = new();

    private ClassSectionSubjectOffering() { }

    public ClassSectionSubjectOffering(
        SubjectId subjectId,
        decimal? subjectUnitsOverride,
        TeacherId? teacherId,
        ClassSectionId classSectionId,
        RoomId roomId,
        int daysPerWeek = 1, // default 1 day per week
        double hoursPerDay = 1, // default 1 hour per day
        int? maxNumberOfStudents = null)
    {
        SubjectId = Guard.Against.Null(subjectId, nameof(subjectId));
        SubjectUnitsOverride = subjectUnitsOverride;
        TeacherId = teacherId;
        ClassSectionId = Guard.Against.Null(classSectionId, nameof(classSectionId));
        RoomId = Guard.Against.Null(roomId, nameof(roomId));
        DaysPerWeek = Guard.Against.NegativeOrZero(daysPerWeek, nameof(daysPerWeek));
        HoursPerDay = Guard.Against.NegativeOrZero(hoursPerDay, nameof(hoursPerDay));
        MaxNumberOfStudents = maxNumberOfStudents;
    }

    public SubjectId SubjectId { get; private set; }
    public Subject? Subject { get; private set; }

    public decimal? SubjectUnitsOverride { get; private set; }

    public TeacherId? TeacherId { get; private set; }
    public Teacher? Teacher { get; private set; }

    public ClassSectionId ClassSectionId { get; private set; }

    public RoomId RoomId { get; private set; }
    public Room? Room { get; private set; }

    public int DaysPerWeek { get; private set; }
    public double HoursPerDay { get; private set; }
    public int? MaxNumberOfStudents { get; private set; }

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

    public ClassSectionSubjectOffering UpdateSubjectUnitsOverride (decimal? unitsOverride)
    {
        if (unitsOverride == SubjectUnitsOverride) return this;
        SubjectUnitsOverride = unitsOverride;
        return this;
    }

    public ClassSectionSubjectOffering UpdateTeacher (TeacherId newTeacherId)
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

    public ClassSectionSubjectOffering UpdateSchedule(int newDaysPerWeek, double newHoursPerDay)
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

    public bool IsFullyScheduled() => _classSchedules.Count == DaysPerWeek;

    public ClassSectionSubjectOffering AddClassSchedule(ClassSchedule classSchedule)
    {
        Guard.Against.Null(classSchedule, nameof(classSchedule));

        if (_classSchedules.Any(cs => cs.DayOfWeek == classSchedule.DayOfWeek))
        {
            throw new InvalidClassScheduleException($"A class schedule for the same day {classSchedule.DayOfWeek.Value} already exists.");
        }

        var numberOfHours = (classSchedule.EndTime - classSchedule.StartTime).TotalHours;
        var totalHoursAfterAdding = _classSchedules.Sum(cs => (cs.EndTime - cs.StartTime).TotalHours) + numberOfHours;
        var expectedTotalHours = (double)(DaysPerWeek * HoursPerDay);
        if (_classSchedules.Count + 1 == DaysPerWeek && Math.Abs(totalHoursAfterAdding - expectedTotalHours) > 0.01)
        {
            throw new InvalidClassScheduleException($"Total hours ({totalHoursAfterAdding:F2}) must equal days per week ({DaysPerWeek}) × hours per day ({HoursPerDay}) = {expectedTotalHours:F2} hours.");
        }

        _classSchedules.Add(classSchedule);
        return this;
    }

    public ClassSectionSubjectOffering UpdateClassSchedule(ClassScheduleId classScheduleId, TimeOnly newStartTime, TimeOnly newEndTime)
    {
        Guard.Against.Null(classScheduleId, nameof(classScheduleId));
        Guard.Against.Default(newStartTime, nameof(newStartTime));
        Guard.Against.Default(newEndTime, nameof(newEndTime));

        var existingSchedule = _classSchedules.FirstOrDefault(cs => cs.Id == classScheduleId);
        if (existingSchedule is null)
        {
            throw new InvalidClassScheduleException($"Class schedule with ID {classScheduleId.Value} not found.");
        }

        // Check if the new times are different
        if (existingSchedule.StartTime == newStartTime && existingSchedule.EndTime == newEndTime)
        {
            return this;
        }

        // Validate end time is after start time
        Guard.Against.InvalidInput(newEndTime, nameof(newEndTime), e => e > newStartTime, "End time must be after start time.");

        // Calculate new total hours after update
        var newScheduleDuration = (newEndTime - newStartTime).TotalHours;
        var totalHoursAfterUpdate = _classSchedules
            .Where(cs => cs.Id != classScheduleId)
            .Sum(cs => (cs.EndTime - cs.StartTime).TotalHours) + newScheduleDuration;

        var expectedTotalHours = (double)(DaysPerWeek * HoursPerDay);

        // Only validate total hours if all schedules are present
        if (_classSchedules.Count == DaysPerWeek && Math.Abs(totalHoursAfterUpdate - expectedTotalHours) > 0.01)
        {
            throw new InvalidClassScheduleException($"Total hours after update ({totalHoursAfterUpdate:F2}) must equal days per week ({DaysPerWeek}) × hours per day ({HoursPerDay}) = {expectedTotalHours:F2} hours.");
        }

        // Remove old schedule and add updated one
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

        var existingSchedule = _classSchedules.FirstOrDefault(cs => cs.Id == classScheduleId);
        if (existingSchedule is null)
        {
            throw new InvalidClassScheduleException($"Class schedule with ID {classScheduleId.Value} not found.");
        }

        _classSchedules.Remove(existingSchedule);
        return this;
    }


}
