using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

public class ClassSchedule : EntityBase<ClassSchedule, ClassScheduleId>, IAuditable
{
    private ClassSchedule() { }

    public ClassSchedule(
        ClassSectionSubjectOfferingId classSectionSubjectOfferingId, DayOfWeekEnum dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        ClassSectionSubjectOfferingId = Guard.Against.Null(classSectionSubjectOfferingId, nameof(classSectionSubjectOfferingId));
        DayOfWeek = Guard.Against.Null(dayOfWeek, nameof(dayOfWeek));
        StartTime = Guard.Against.Default(startTime, nameof(startTime));
        EndTime = Guard.Against.Default(endTime, nameof(endTime));

        Guard.Against.InvalidInput(endTime, nameof(endTime), e => e > startTime, "End time must be after start time.");
    }

    public ClassSectionSubjectOfferingId ClassSectionSubjectOfferingId { get; private set; }

    public DayOfWeekEnum DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

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

    public ClassSchedule UpdateDayOfWeek (DayOfWeekEnum newDayOfWeek)
    {
        if (DayOfWeek == newDayOfWeek) return this;
        DayOfWeek = newDayOfWeek;
        return this;
    }

    public ClassSchedule UpdateStartAndEndTime(TimeOnly newStartTime, TimeOnly newEndTime)
    {
        if (StartTime == newStartTime && EndTime == newEndTime) return this;
        Guard.Against.Default(newStartTime, nameof(newStartTime));
        Guard.Against.Default(newEndTime, nameof(newEndTime));
        Guard.Against.InvalidInput(newEndTime, nameof(newEndTime), e => e > newStartTime, "End time must be after start time.");
        StartTime = newStartTime;
        EndTime = newEndTime;
        return this;
    }

}
