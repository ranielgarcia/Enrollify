using Enhance_Genetic_Algorithm_v2.Models;

namespace Enhance_Genetic_Algorithm_v2;
public class ConflictDetector
{
    public static List<string> DetectAllConflicts(Schedule schedule, ScheduleData data)
    {
        var conflicts = new List<string>();

        // Section conflicts
        var sectionConflicts = DetectSectionConflicts(schedule);
        conflicts.AddRange(sectionConflicts);

        var lessOrMoreThanSubjectSchedules = DetectSubjectsScheduledLessOrMoreThanDaysPerWeek(schedule);
        conflicts.AddRange(lessOrMoreThanSubjectSchedules);

        var invalidTimeSlots = DetectTimeSlotInvalid(schedule);
        conflicts.AddRange(invalidTimeSlots);

        // Professor conflicts
        var professorConflicts = DetectProfessorConflicts(schedule);
        conflicts.AddRange(professorConflicts);

        // Room conflicts
        var roomConflicts = DetectRoomConflicts(schedule);
        conflicts.AddRange(roomConflicts);

        // Room capacity violations
        var capacityViolations = DetectCapacityViolations(schedule);
        conflicts.AddRange(capacityViolations);

        // Room type incompatibility
        var roomTypeViolations = DetectRoomTypeViolations(schedule);
        conflicts.AddRange(roomTypeViolations);

        // Professor qualification issues
        var qualificationIssues = DetectQualificationIssues(schedule);
        conflicts.AddRange(qualificationIssues);

        return conflicts;
    }

    private static List<string> DetectSectionConflicts(Schedule schedule)
    {
        var conflicts = new List<string>();
        var sectionSchedule = schedule.Genes.GroupBy(g => g.Section.Id);

        foreach (var sectionGroup in sectionSchedule)
        {
            var timeGroups = sectionGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                {
                    var section = timeGroup.First().Section;
                    var subjects = string.Join(", ", timeGroup.Select(g => g.Subject.Code));
                    var time = timeGroup.First().TimeSlot;
                    conflicts.Add($"SECTION CONFLICT: {section.Name} has {timeGroup.Count()} subjects at {time}: {subjects}");
                }
            }
        }

        return conflicts;
    }

    private static List<string> DetectSubjectsScheduledLessOrMoreThanDaysPerWeek(Schedule schedule)
    {
        var conflicts = new List<string>();
        var subjectSectionSchedule = schedule.Genes.GroupBy(g => new { subjectId = g.Subject.Id, sectionId = g.Section.Id });
        foreach (var group in subjectSectionSchedule)
        {
            var subject = group.First().Subject;
            var actualDays = group.Select(g => g.TimeSlot.Day).Distinct().Count();

            if (actualDays != subject.DaysPerWeek)
            {
                conflicts.Add($"SUBJECT SCHEDULED LESS/MORE THAN DaysPerWeek: {subject.Name} has {actualDays} days scheduled versus {subject.DaysPerWeek} required days");
            }
        }
        return conflicts;
    }

    private static List<string> DetectTimeSlotInvalid(Schedule schedule)
    {
        var conflicts = new List<string>();
        foreach (var gene in schedule.Genes)
        {
            var slotDuration = gene.TimeSlot.GetDurationHours();
            var requiredDuration = gene.Subject.HoursPerDay;

            if (Math.Abs(slotDuration - requiredDuration) > 0.1) // Allow small tolerance
            {
                conflicts.Add($"TimeSlot Invalid: {gene.Subject.Name} has {slotDuration} timeslot scheduled versus {requiredDuration} required timeslot");
            }
        }
        return conflicts;
    }

    private static List<string> DetectProfessorConflicts(Schedule schedule)
    {
        var conflicts = new List<string>();
        var professorSchedule = schedule.Genes.GroupBy(g => g.ProfessorId);

        foreach (var profGroup in professorSchedule)
        {
            var timeGroups = profGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                {
                    var classes = string.Join(", ", timeGroup.Select(g => $"{g.Subject.Code}({g.Section.Name})"));
                    var time = timeGroup.First().TimeSlot;
                    conflicts.Add($"PROFESSOR CONFLICT: Prof. {profGroup.Key} teaches {timeGroup.Count()} classes at {time}: {classes}");
                }
            }
        }

        return conflicts;
    }

    private static List<string> DetectRoomConflicts(Schedule schedule)
    {
        var conflicts = new List<string>();
        var roomSchedule = schedule.Genes.GroupBy(g => new { roomId = g.Room.Id, timeSlotId = g.TimeSlot.Id });

        foreach (var group in roomSchedule)
        {
            if (group.Count() > 1)
            {
                var room = group.First().Room;
                var classes = string.Join(", ", group.Select(g => $"{g.Subject.Code}({g.Section.Name})"));
                var time = group.First().TimeSlot;
                conflicts.Add($"ROOM CONFLICT: {room.Name} has {group.Count()} classes at {time}: {classes}");
            }
        }

        return conflicts;
    }

    private static List<string> DetectCapacityViolations(Schedule schedule)
    {
        var violations = new List<string>();

        foreach (var gene in schedule.Genes)
        {
            if (gene.Room.Capacity < gene.Section.StudentCount)
            {
                violations.Add($"CAPACITY: {gene.Subject.Code}({gene.Section.Name}) has {gene.Section.StudentCount} students but room {gene.Room.Name} only fits {gene.Room.Capacity}");
            }
        }

        return violations;
    }

    private static List<string> DetectRoomTypeViolations(Schedule schedule)
    {
        var violations = new List<string>();

        foreach (var gene in schedule.Genes)
        {
            if (!gene.Room.IsCompatibleWith(gene.Subject))
            {
                violations.Add($"ROOM TYPE: {gene.Subject.Code} ({gene.Subject.Type}) assigned to incompatible room {gene.Room.Name} ({gene.Room.Type})");
            }
        }

        return violations;
    }

    private static List<string> DetectQualificationIssues(Schedule schedule)
    {
        var issues = new List<string>();

        foreach (var gene in schedule.Genes)
        {
            if (!gene.Subject.ProfessorIds.Contains(gene.ProfessorId))
            {
                issues.Add($"QUALIFICATION: Prof. {gene.ProfessorId} is not qualified to teach {gene.Subject.Code}");
            }
        }

        return issues;
    }
}