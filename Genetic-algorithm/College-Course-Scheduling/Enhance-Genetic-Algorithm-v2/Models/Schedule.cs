using static Enhance_Genetic_Algorithm_v2.Constraints;

namespace Enhance_Genetic_Algorithm_v2.Models;

public class Schedule
{
    private readonly GAConfiguration _config;
    public List<Gene> Genes { get; set; }
    private double? _cachedFitness;

    public Schedule(GAConfiguration config = null)
    {
        Genes = new List<Gene>();
        _config = config ?? GAConfiguration.Default;
    }

    public Schedule(List<Gene> genes, GAConfiguration config = null)
    {
        Genes = genes;
        _config = config ?? GAConfiguration.Default;
    }

    public double CalculateFitness(ScheduleData data)
    {
        if (_cachedFitness.HasValue)
            return _cachedFitness.Value;

        double penalty = 0;

        Constraints.RoomCapacityMustFitSectionSize(Genes, (Gene _) => penalty += _config.RoomCapacityPenalty);
        Constraints.RoomTypeCompatibilityWithSubject(Genes, (Gene _) => penalty += _config.RoomTypePenalty);
        Constraints.NoSectionConflicts(Genes, (IGrouping<string, Gene> timeGroup) =>
        {
            penalty += _config.SectionConflictPenalty * (timeGroup.Count() - 1);  // Critical violation
        });
        Constraints.NoProfessorConflicts(Genes, (IGrouping<string, Gene> timeGroup) =>
        {
            penalty += _config.ProfessorConflictPenalty * (timeGroup.Count() - 1);
        });
        Constraints.RoomCannotBeUsedByMultipleSectionsAtTheSameTime(Genes, (IGrouping<RoomTimeSlot, Gene> group) =>
        {
            penalty += _config.RoomConflictPenalty * (group.Count() - 1);
        });
        Constraints.ProfessorCanOnlyTeachSubjectsTheyAreQualifiedFor(Genes, (Gene _) =>
        {
            penalty += _config.QualificationPenalty;
        });
        Constraints.SubjectMustBeScheduledDaysPerWeekTimes(Genes, (Subject subject, int actualDays) =>
        {
            penalty += _config.DaysPerWeekPenalty * Math.Abs(actualDays - subject.DaysPerWeek); // Critical violation
        });
        Constraints.TimeSlotMustAccomodateSubjectsRequiredHoursPerDay(Genes, (Gene _, double _, double _) => {
            penalty += _config.HoursPerDayPenalty; // Heavy penalty for time mismatch
        });
        Constraints.NoOverlappingTimeSlotsWithinSameSection(Genes, (Gene _, Gene _) =>
        {
            penalty += _config.OverlapPenalty; // CRITICAL - overlapping times in same section
        });
        Constraints.SameSubjectShouldHaveSameTimeAcrossDifferentDays(Genes, 
            (IGrouping<SubjectSectionMap, Gene> _, int _) =>
        {
            penalty += _config.TimePatternPenalty; // Prefer consistent times (e.g., always at 9:00-10:30)
        });
        Constraints.RespectPreferredDayPatterns(Genes, (List<Gene> _) =>
        {
            penalty += _config.DayPatternPenalty;
        });
        Constraints.MinimizeGapsInProfessorSchedulesPerDay(Genes, () =>
        {
            penalty += _config.ProfessorGapPenalty;  // Small penalty for each gap
        });
        // Disabled
        //Constraints.PreferMorningClasses(Genes, () =>
        //{
        //    penalty += _config.AfternoonPenalty;
        //});

        Constraints.SameSectionShouldHaveClassesInNearbyRooms(Genes, () =>
        {
            penalty += _config.RoomProximityPenalty;
        });

        Constraints.SubjectTimeSlotShouldBeWithinSubjectTimePreference(Genes, () =>
        {
            penalty += _config.TimePreferencePenalty; // Penalty for scheduling outside preferred time
        });

        // Normalized fitness (0-1000 range)
        double maxPenalty = CalculateMaxPossiblePenalty(data);
        _cachedFitness = 1000 * (1 - (penalty / maxPenalty));
        return Math.Max(_cachedFitness.Value, 0);
    }

    private double CalculateMaxPossiblePenalty(ScheduleData data)
    {
        int totalSessions = data.Sections.Sum(s =>
            s.SubjectIds.Sum(subId =>
                data.Subjects.First(sub => sub.Id == subId).DaysPerWeek));

        double maxPenalty = 0;
        maxPenalty += totalSessions * _config.RoomCapacityPenalty;   // Room capacity
        maxPenalty += totalSessions * _config.RoomTypePenalty;   // Room type compatibility
        maxPenalty += totalSessions * _config.SectionConflictPenalty;   // Section conflicts
        maxPenalty += totalSessions * _config.OverlapPenalty;   // Overlapping timeslots (NEW)
        maxPenalty += totalSessions * _config.ProfessorConflictPenalty;   // Professor conflicts
        maxPenalty += totalSessions * _config.RoomConflictPenalty;   // Room conflicts
        maxPenalty += totalSessions * _config.QualificationPenalty;   // Professor qualification
        maxPenalty += totalSessions * _config.DaysPerWeekPenalty;   // DaysPerWeek violation
        maxPenalty += totalSessions * _config.HoursPerDayPenalty;   // HoursPerDay mismatch
        maxPenalty += totalSessions * _config.TimePreferencePenalty;    // Time preference violation (NEW)
        maxPenalty += totalSessions * _config.TimePatternPenalty;    // Time Pattern Consistency
        maxPenalty += totalSessions * _config.DayPatternPenalty;    // Day pattern consistency
        maxPenalty += totalSessions * _config.ProfessorGapPenalty;    // Professor Gap
        maxPenalty += totalSessions * _config.AfternoonPenalty;
        maxPenalty += totalSessions * _config.RoomProximityPenalty;
        maxPenalty += totalSessions * 10;    // Day pattern preference
        maxPenalty += data.Sections.Count * 5;

        return maxPenalty;
    }

    public int GetViolationCount(ScheduleData data)
    {
        int violations = 0;

        // Room capacity
        foreach (var gene in Genes)
        {
            if (gene.Room.Capacity < gene.Section.StudentCount)
                violations++;
        }

        // Room type compatibility
        foreach (var gene in Genes)
        {
            if (!gene.Room.IsCompatibleWith(gene.Subject))
                violations++;
        }

        // Section conflicts
        var sectionSchedule = Genes.GroupBy(g => g.Section.Id);
        foreach (var sectionGroup in sectionSchedule)
        {
            var timeGroups = sectionGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                    violations += timeGroup.Count() - 1;
            }
        }

        // Professor conflicts
        var professorSchedule = Genes.GroupBy(g => g.ProfessorId);
        foreach (var profGroup in professorSchedule)
        {
            var timeGroups = profGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                    violations += timeGroup.Count() - 1;
            }
        }

        // Room conflicts
        var roomSchedule = Genes.GroupBy(g => new { roomId = g.Room.Id, timeSlotId = g.TimeSlot.Id });
        foreach (var group in roomSchedule)
        {
            if (group.Count() > 1)
                violations += group.Count() - 1;
        }

        // Professor qualification
        foreach (var gene in Genes)
        {
            if (!gene.Subject.ProfessorIds.Contains(gene.ProfessorId))
                violations++;
        }

        return violations;
    }

    public Schedule Clone()
    {
        var newGenes = Genes.Select(g => g.Clone()).ToList();
        return new Schedule(newGenes, _config);
    }

    //public void Display()
    //{
    //    Console.WriteLine($"{"Subject",-10} | {"Section",-12} | {"Room",-15} | {"Time Slot",-20} | Professor");
    //    Console.WriteLine(new string('-', 95));

    //    foreach (var gene in Genes.OrderBy(g => g.Section.Name)
    //                              .ThenBy(g => g.TimeSlot.Day)
    //                              .ThenBy(g => g.TimeSlot.StartTime))
    //    {
    //        Console.WriteLine(gene.ToString());
    //    }
    //}

    public void DisplayBySection(ScheduleData data)
    {
        var sections = Genes.GroupBy(g => g.Section.Id).OrderBy(g => g.Key);

        foreach (var sectionGroup in sections)
        {
            var section = data.Sections.First(s => s.Id == sectionGroup.Key);
            Console.WriteLine($"\n{'█'} {section.Name} - {section.StudentCount} students");
            Console.WriteLine(new string('─', 95));

            foreach (var gene in sectionGroup.OrderBy(g => g.TimeSlot.Day)
                                              .ThenBy(g => g.TimeSlot.StartTime))
            {
                Console.WriteLine($"  {gene.Subject.Code,-10} | {gene.Room.Name,-15} | {gene.TimeSlot,-20} | Prof. {gene.ProfessorId}");
            }
        }
    }
}