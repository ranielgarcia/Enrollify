using Enhance_Genetic_Algorithm_v2.Models;
using static Enhance_Genetic_Algorithm_v2.Constraints;

namespace Enhance_Genetic_Algorithm_v2;
public class ConflictDetector
{
    public static List<string> DetectAllConflicts(Schedule schedule, ScheduleData data)
    {
        var conflicts = new List<string>();


        var capacityViolations = DetectRoomCapacityViolations(schedule);
        conflicts.AddRange(capacityViolations);

        var roomTypeViolations = DetectRoomTypeViolations(schedule);
        conflicts.AddRange(roomTypeViolations);

        var sectionConflicts = DetectSectionConflicts(schedule);
        conflicts.AddRange(sectionConflicts);

        var professorConflicts = DetectProfessorConflicts(schedule);
        conflicts.AddRange(professorConflicts);

        var lessOrMoreThanSubjectSchedules = DetectSubjectsScheduledLessOrMoreThanDaysPerWeek(schedule);
        conflicts.AddRange(lessOrMoreThanSubjectSchedules);

        var invalidTimeSlots = DetectTimeSlotInvalid(schedule);
        conflicts.AddRange(invalidTimeSlots);


        var roomConflicts = DetectRoomConflicts(schedule);
        conflicts.AddRange(roomConflicts);

        var qualificationViolations = DetectQualificationViolations(schedule);
        conflicts.AddRange(qualificationViolations);

        var overlappingViolations = DetectOverlappingTimeSlotsWithinSameSection(schedule);
        conflicts.AddRange(overlappingViolations);

        var inconsistentSubjectTimesViolations = DetectInconsistentSubjectTimes(schedule);
        conflicts.AddRange(inconsistentSubjectTimesViolations);

        var subjectPreferredDaysViolations = DetectUnsatisfiedSubjectPreferredDayPattern(schedule);
        conflicts.AddRange(subjectPreferredDaysViolations);

        var exceededProfessorMaxTimeSlotsPerDayViolations = DetectExceededProfessorMaxTimeSlotsPerDay(schedule, (string professorId) => data.Professors.First(p => p.Id == professorId).MaxTimeSlotsPerDay);
        conflicts.AddRange(exceededProfessorMaxTimeSlotsPerDayViolations);

        return conflicts;
    }

    private static List<string> DetectSectionConflicts(Schedule schedule)
    {
        // (same section, different subjects, same time)
        var conflicts = new List<string>();
        Constraints.NoSectionConflicts(schedule.Genes, (IGrouping<string, Gene> timeGroup) =>
        {
            var section = timeGroup.First().Section;
            var subjects = string.Join(", ", timeGroup.Select(g => g.Subject.Code));
            var time = timeGroup.First().TimeSlot;
            conflicts.Add($"SECTION CONFLICT: {section.Name} has {timeGroup.Count()} subjects at {time}: {subjects}");
        });
        return conflicts;
    }

    private static List<string> DetectSubjectsScheduledLessOrMoreThanDaysPerWeek(Schedule schedule)
    {
        var conflicts = new List<string>();

        Constraints.SubjectMustBeScheduledDaysPerWeekTimes(schedule.Genes, (Subject subject, int actualDays) =>
        {
            conflicts.Add($"SUBJECT SCHEDULED LESS/MORE THAN DaysPerWeek: {subject.Name} has {actualDays} days scheduled versus {subject.DayAndTimePreference.DaysPerWeek} required days");
        });
        return conflicts;
    }

    private static List<string> DetectTimeSlotInvalid(Schedule schedule)
    {
        var conflicts = new List<string>();
        Constraints.TimeSlotMustAccomodateSubjectsRequiredHoursPerDay(schedule.Genes, (Gene gene, double slotDuration, double requiredDuration) =>
        {
            conflicts.Add($"TimeSlot Invalid: {gene.Subject.Name} has {slotDuration} timeslot scheduled versus {requiredDuration} required timeslot");
        });

        return conflicts;
    }

    private static List<string> DetectProfessorConflicts(Schedule schedule)
    {
        var conflicts = new List<string>();

        Constraints.NoProfessorConflicts(schedule.Genes, (IGrouping<string, Gene> timeGroup) =>
        {
            var classes = string.Join(", ", timeGroup.Select(g => $"{g.Subject.Code}({g.Section.Name})"));
            var time = timeGroup.First().TimeSlot;
            conflicts.Add($"PROFESSOR CONFLICT: Prof. {timeGroup.First().ProfessorId} teaches {timeGroup.Count()} classes at {time}: {classes}");
        });

        return conflicts;
    }

    private static List<string> DetectRoomConflicts(Schedule schedule)
    {
        // Room cannot be used by multiple sections at same time
        var conflicts = new List<string>();

        Constraints.RoomCannotBeUsedByMultipleSectionsAtTheSameTime(schedule.Genes, (IGrouping<RoomTimeSlot, Gene> schedule) =>
        {
            var room = schedule.First().Room;
            var classes = string.Join(", ", schedule.Select(g => $"{g.Subject.Code}({g.Section.Name})"));
            var time = schedule.First().TimeSlot;
            conflicts.Add($"ROOM CONFLICT: {room.Name} has {schedule.Count()} classes at {time}: {classes}");
        });

        return conflicts;
    }

    private static List<string> DetectRoomCapacityViolations(Schedule schedule)
    {
        var violations = new List<string>();
        Constraints.RoomCapacityMustFitSectionSize(schedule.Genes, (Gene gene) =>
        {
            violations.Add($"CAPACITY: {gene.Subject.Code}({gene.Section.Name}) has {gene.Section.StudentCount} students but room {gene.Room.Name} only fits {gene.Room.Capacity}");
        });
        return violations;
    }

    private static List<string> DetectRoomTypeViolations(Schedule schedule)
    {
        var violations = new List<string>();
        Constraints.RoomTypeCompatibilityWithSubject(schedule.Genes, (Gene gene) =>
        {
            violations.Add($"ROOM TYPE: {gene.Subject.Code} ({gene.Subject.Type}) assigned to incompatible room {gene.Room.Name} ({gene.Room.Type})");
        });
        return violations;
    }

    private static List<string> DetectQualificationViolations(Schedule schedule)
    {
        var violations = new List<string>();

        Constraints.ProfessorCanOnlyTeachSubjectsTheyAreQualifiedFor(schedule.Genes, (Gene gene) =>
        {
            violations.Add($"QUALIFICATION: Prof. {gene.ProfessorId} is not qualified to teach {gene.Subject.Code}");
        });

        return violations;
    }

    private static List<string> DetectOverlappingTimeSlotsWithinSameSection(Schedule schedule)
    {
        var violations = new List<string>();

        Constraints.NoOverlappingTimeSlotsWithinSameSection(schedule.Genes, (Gene gene1, Gene gene2) =>
        {
            violations.Add($"OVERLAPPING SCHEDULE: Subject {gene1.Subject.Name} ({gene1.TimeSlot.StartTime}-{gene1.TimeSlot.EndTime}) overlapped with Subject {gene2.Subject.Name} ({gene2.TimeSlot.StartTime}-{gene2.TimeSlot.EndTime})");
        });

        return violations;
    }

    private static List<string> DetectInconsistentSubjectTimes(Schedule schedule)
    {
        var violations = new List<string>();

        Constraints.SameSubjectShouldHaveSameTimeAcrossDifferentDays(schedule.Genes, 
            (IGrouping<SubjectSectionMap, Gene> schedules, int count) =>
        {
            var subject = schedules.First().Subject;
            var section = schedules.First().Section;
            var timeDetails = string.Join(", ", schedules.Select(g => $"{g.TimeSlot.Day} {g.TimeSlot.GetTimePattern()}"));

            violations.Add($"(Soft constraint) INCONSISTENT TIME: {subject.Code} for {section.Name} has {count} different time patterns: {timeDetails}");
        });

        return violations;
    }

    private static List<string> DetectUnsatisfiedSubjectPreferredDayPattern(Schedule schedule)
    {
        var violations = new List<string>();

        Constraints.RespectPreferredDayPatterns(schedule.Genes, (List<Gene> subjectSectionGenes) =>
        {
            var subject = subjectSectionGenes.First().Subject;
            var section = subjectSectionGenes.First().Section;
            var days = subjectSectionGenes.Select(g => g.TimeSlot.Day).ToList();
            var actualDays = string.Join(", ", days.Distinct());
            
            violations.Add($"(Soft constraint) PREFERRED DAY PATTERN: {subject.Code} for {section.Name} prefers {subject.DayAndTimePreference.PreferredDayPattern} but is scheduled on: {actualDays}");
        });

        return violations;
    }

    private static List<string> DetectExceededProfessorMaxTimeSlotsPerDay(Schedule schedule, Func<string, int> getProfessorMaxTimeSlotsPerDay)
    {
        var violations = new List<string>();

        Constraints.NoExceedingProfessorMaxTimeSlotsPerDay(schedule.Genes, getProfessorMaxTimeSlotsPerDay, 
            (string professorId, IGrouping<string, Gene> genes) =>
            {
                var professorMaxTimeSlotsPerDay = getProfessorMaxTimeSlotsPerDay(professorId);
                var timeSlotCount = genes.Count();
                var day = genes.First().TimeSlot.Day;

                violations.Add($"PROFESSOR MAX TIMESLOTS PER DAY EXCEEDED: {professorId} with MaxTimeSlotsPerDay of {professorMaxTimeSlotsPerDay} has {timeSlotCount} alloted slots in {day}");
            });

        return violations;
    }
}