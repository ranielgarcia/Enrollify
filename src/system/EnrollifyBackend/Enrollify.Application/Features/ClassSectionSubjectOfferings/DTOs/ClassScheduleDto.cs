using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.DTOs;

public class ClassScheduleDto
{
    public int Id { get; set; }
    public string DayOfWeek { get; set; } = null!;
    public string DayOfWeekAbbreviation { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public static ClassScheduleDto FromEntity(ClassSchedule schedule)
    {
        return new ClassScheduleDto
        {
            Id = schedule.Id.Value,
            DayOfWeek = schedule.DayOfWeek.Name,
            DayOfWeekAbbreviation = schedule.DayOfWeek.Value,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
        };
    }
}
