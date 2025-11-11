using Enhance_Genetic_Algorithm_v2.Models;

namespace Enhance_Genetic_Algorithm_v2;
public static class Constraints
{
    public static void RoomCapacityMustFitSectionSize (List<Gene> genes, Action<Gene> action)
    {
        // Hard Constraint 1: Room capacity must fit section size
        foreach (var gene in genes)
        {
            if (gene.Room.Capacity < gene.Section.StudentCount)
                action(gene);
        }
    }//

    public static void RoomTypeCompatibilityWithSubject(List<Gene> genes, Action<Gene> action)
    {
        // Hard Constraint 2: Room type compatibility with subject
        foreach (var gene in genes)
        {
            if (!gene.Room.IsCompatibleWith(gene.Subject))
                action(gene);
        }
    }//

    public static void NoSectionConflicts(List<Gene> genes, Action<IGrouping<string, Gene>> action)
    {
        // Hard Constraint 3: No section conflicts (same section, different subjects, same time)
        var sectionSchedule = genes.GroupBy(g => g.Section.Id);
        foreach (var sectionGroup in sectionSchedule)
        {
            var timeGroups = sectionGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                    action(timeGroup);
            }
        }
    }//

    public static void NoProfessorConflicts(List<Gene> genes, Action<IGrouping<string, Gene>> action) 
    {
        // Hard Constraint 4: Professor cannot teach multiple classes at same time
        var professorSchedule = genes.GroupBy(g => g.ProfessorId);
        foreach (var profGroup in professorSchedule)
        {
            var timeGroups = profGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                    action(timeGroup);
            }
        }
    }//

    public static void NoExceedingProfessorMaxTimeSlotsPerDay(
        List<Gene> genes, 
        Func<string, int> getProfessorMaxTimeSlotsPerDay, 
        Action<string, IGrouping<string, Gene>> action)
    {
        // Hard Constraint 4: Professor cannot teach classes more than its MaxTimeSlotsPerDay
        var professorSchedule = genes.GroupBy(g => g.ProfessorId);
        foreach (var profGroup in professorSchedule)
        {
            var timeGroups = profGroup.GroupBy(g => g.TimeSlot.Day);
            foreach (var timeGroup in timeGroups)
            {
                var professorMaxTimeSlotsPerDay = getProfessorMaxTimeSlotsPerDay(profGroup.First().ProfessorId);

                if (timeGroup.Count() > professorMaxTimeSlotsPerDay)
                    action(profGroup.First().ProfessorId, timeGroup);
            }
        }
    }//


    public record RoomTimeSlot(string roomId, string timeSlotId);
    public static void RoomCannotBeUsedByMultipleSectionsAtTheSameTime(List<Gene> genes, Action<IGrouping<RoomTimeSlot, Gene>> action)
    {
        // Hard Constraint 5: Room cannot be used by multiple sections at same time
        var roomSchedules = genes.GroupBy(g => new RoomTimeSlot (g.Room.Id, g.TimeSlot.Id));
        foreach (var schedule in roomSchedules)
        {
            if (schedule.Count() > 1)
                action(schedule);
        }
    }// 

    public static void ProfessorCanOnlyTeachSubjectsTheyAreQualifiedFor(List<Gene> genes, Action<Gene> action)
    {
        // Hard Constraint 6: Professor can only teach subjects they're qualified for
        foreach (var gene in genes)
        {
            if (!gene.Subject.ProfessorIds.Contains(gene.ProfessorId))
                action(gene);
        }
    } //

    public static void SubjectMustBeScheduledDaysPerWeekTimes(List<Gene> genes, Action<Subject, int> action)
    {
        // Hard Constraint 7: Subject must be scheduled DaysPerWeek times
        var subjectSectionSchedule = genes.GroupBy(g => new { subjectId = g.Subject.Id, sectionId = g.Section.Id });
        foreach (var group in subjectSectionSchedule)
        {
            var subject = group.First().Subject;
            var actualDays = group.Select(g => g.TimeSlot.Day).Distinct().Count();

            if (subject != null && actualDays != subject.DayAndTimePreference.DaysPerWeek)
            {
                action(subject, actualDays);
            }
        }
    }//

    public static void TimeSlotMustAccomodateSubjectsRequiredHoursPerDay(List<Gene> genes, Action<Gene, double, double> action)
    {
        foreach (var gene in genes)
        {
            var slotDuration = gene.TimeSlot.GetDurationHours();
            var requiredDuration = gene.Subject.HoursPerDay;

            if (Math.Abs(slotDuration - requiredDuration) > 0.1) // Allow small tolerance
            {
                action(gene, slotDuration, requiredDuration);
            }
        }
    }//

    public static void NoOverlappingTimeSlotsWithinSameSection(List<Gene> genes, Action<Gene, Gene> action)
    {
        // Hard Constraint: No overlapping timeslots within same section
        // e.g. Subject 1 starts at 08:00 and ends at 10:00
        // e.g. Overlapping subject 2 starts at 09:30 and ends at 10:30
        var sectionOverlaps = genes.GroupBy(g => g.Section.Id);
        foreach (var sectionGroup in sectionOverlaps)
        {
            var genesList = sectionGroup.ToList();
            for (int i = 0; i < genesList.Count; i++)
            {
                for (int j = i + 1; j < genesList.Count; j++)
                {
                    if (genesList[i].TimeSlot.OverlapsWith(genesList[j].TimeSlot))
                    {
                        action(genesList[i], genesList[j]);
                    }
                }
            }
        }
    }//

    public record SubjectSectionMap(string subjectId, string sectionId);
    public static void SameSubjectShouldHaveSameTimeAcrossDifferentDays(
        List<Gene> genes, 
        Action<IGrouping<SubjectSectionMap, Gene>, int> action)
    {
        // Soft Constraint 1: Same subject should have same time across different days
        // Prefer consistent times (e.g., always at 9:00-10:30)
        var subjectSectionSchedule = genes.GroupBy(g => new SubjectSectionMap (g.Subject.Id,g.Section.Id ));
        foreach (var group in subjectSectionSchedule)
        {
            if (group.Count() > 1)
            {
                var times = group.Select(g => g.TimeSlot.GetTimePattern()).Distinct().ToList();
                if (times.Count > 1)
                {
                    action(group, times.Count);
                }
            }
        }
    } //


    public static void RespectPreferredDayPatterns (List<Gene> genes, Action<List<Gene>> action)
    {
        // Soft Constraint 2: Respect preferred day patterns (MW, TTh, etc.)
        foreach (var gene in genes)
        {
            var (matchesPattern, relatedGenes) = CheckDayPattern(gene, genes);
            if (!matchesPattern)
                action(relatedGenes);
        }
    } //

    public static void MinimizeGapsInProfessorSchedulesPerDay(List<Gene> genes, Action action)
    {
        var professorSchedule = genes.GroupBy(g => g.ProfessorId);
        // Soft Constraint 3: Minimize gaps in professor schedules per day
        foreach (var profGroup in professorSchedule)
        {
            var dayGroups = profGroup.GroupBy(g => g.TimeSlot.Day);
            foreach (var dayGroup in dayGroups)
            {
                if (dayGroup.Count() > 1)
                {
                    var slots = dayGroup.OrderBy(g => g.TimeSlot.StartTime).ToList();
                    // Check for gaps between consecutive classes
                    for (int i = 0; i < slots.Count - 1; i++)
                    {
                        action();
                    }
                }
            }
        }
    }

    // I don't need this right now
    public static void PreferMorningClasses (List<Gene> genes, Action action)
    {
        // Soft Constraint 4: Prefer morning classes (before 13:00)
        foreach (var gene in genes)
        {
            if (string.Compare(gene.TimeSlot.StartTime, "13:00") >= 0)
                action();
        }
    }


    public static void SameSectionShouldHaveClassesInNearbyRooms(List<Gene> genes, Action action)
    {
        var sectionSchedule = genes.GroupBy(g => g.Section.Id);
        // Soft Constraint 5: Same section should have classes in nearby rooms
        foreach (var sectionGroup in sectionSchedule)
        {
            var rooms = sectionGroup.Select(g => g.Room.Id).Distinct().ToList();
            if (rooms.Count > 5)  // Too many different rooms
                action();
        }
    }

    public static void SubjectTimeSlotShouldBeWithinSubjectTimePreference(List<Gene> genes, Action action)
    {
        // Soft Constraint 6: Subject time preferences (e.g., only 7am-6pm)
        foreach (var gene in genes)
        {
            if (!gene.Subject.DayAndTimePreference.IsWithinPreference(gene.TimeSlot))
            {
                action();
            }
        }
    }


    public static (bool isMet, List<Gene> relatedGenes) CheckDayPattern(Gene gene, List<Gene> allGenes)
    {
        var subject = gene.Subject;
        var section = gene.Section;

        // Get all genes for same subject-section combination
        var relatedGenes = allGenes.Where(g =>
            g.Subject.Id == subject.Id &&
            g.Section.Id == section.Id).ToList();

        var days = relatedGenes.Select(g => g.TimeSlot.Day).ToList();
        if (relatedGenes.Count <= 1)
            return (true, relatedGenes);  // Single session, no pattern to check

        switch (subject.DayAndTimePreference.PreferredDayPattern)
        {
            case DayPattern.MW:
                return (days.All(d => d == "Monday" || d == "Wednesday"), relatedGenes);

            case DayPattern.TTh:
                return (days.All(d => d == "Tuesday" || d == "Thursday"), relatedGenes);

            case DayPattern.MWF:
                return (days.All(d => d == "Monday" || d == "Wednesday" || d == "Friday"), relatedGenes);

            case DayPattern.Daily:
                return (true, relatedGenes);  // Any day is fine

            case DayPattern.Single:
                return (days.Distinct().Count() == 1, relatedGenes);

            default:
                return (true, relatedGenes);
        }
    }

}
