namespace Enhance_Genetic_Algorithm_v2.Models;

public class Schedule
{
    public List<Gene> Genes { get; set; }
    private double? _cachedFitness;

    public Schedule()
    {
        Genes = new List<Gene>();
    }

    public Schedule(List<Gene> genes)
    {
        Genes = genes;
    }

    public double CalculateFitness(ScheduleData data)
    {
        if (_cachedFitness.HasValue)
            return _cachedFitness.Value;

        double penalty = 0;

        // Hard Constraint 1: Room capacity must fit section size
        foreach (var gene in Genes)
        {
            if (gene.Room.Capacity < gene.Section.StudentCount)
                penalty += 100;
        }

        // Hard Constraint 2: Room type compatibility with subject
        foreach (var gene in Genes)
        {
            if (!gene.Room.IsCompatibleWith(gene.Subject))
                penalty += 150;
        }

        // Hard Constraint 3: No section conflicts (same section, different subjects, same time)
        var sectionSchedule = Genes.GroupBy(g => g.Section.Id);
        foreach (var sectionGroup in sectionSchedule)
        {
            var timeGroups = sectionGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                    penalty += 200 * (timeGroup.Count() - 1);  // Critical violation
            }
        }

        // Hard Constraint 4: Professor cannot teach multiple classes at same time
        var professorSchedule = Genes.GroupBy(g => g.ProfessorId);
        foreach (var profGroup in professorSchedule)
        {
            var timeGroups = profGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeGroups)
            {
                if (timeGroup.Count() > 1)
                    penalty += 100 * (timeGroup.Count() - 1);
            }
        }

        // Hard Constraint 5: Room cannot be used by multiple sections at same time
        var roomSchedule = Genes.GroupBy(g => new { roomId = g.Room.Id, timeSlotId = g.TimeSlot.Id });
        foreach (var group in roomSchedule)
        {
            if (group.Count() > 1)
                penalty += 100 * (group.Count() - 1);
        }

        // Hard Constraint 6: Professor can only teach subjects they're qualified for
        foreach (var gene in Genes)
        {
            if (!gene.Subject.ProfessorIds.Contains(gene.ProfessorId))
                penalty += 150;
        }

        // Hard Constraint 7: Subject must be scheduled DaysPerWeek times
        var subjectSectionSchedule = Genes.GroupBy(g => new { subjectId = g.Subject.Id, sectionId = g.Section.Id });
        foreach (var group in subjectSectionSchedule)
        {
            var subject = group.First().Subject;
            var actualDays = group.Select(g => g.TimeSlot.Day).Distinct().Count();

            if (actualDays != subject.DaysPerWeek)
            {
                penalty += 200 * Math.Abs(actualDays - subject.DaysPerWeek); // Critical violation
            }
        }

        // Hard Constraint 8: TimeSlot must accommodate HoursPerDay
        foreach (var gene in Genes)
        {
            var slotDuration = gene.TimeSlot.GetDurationHours();
            var requiredDuration = gene.Subject.HoursPerDay;

            if (Math.Abs(slotDuration - requiredDuration) > 0.1) // Allow small tolerance
            {
                penalty += 150; // Heavy penalty for time mismatch
            }
        }

        // Soft Constraint 1: Same subject should have same time across different days
        foreach (var group in subjectSectionSchedule)
        {
            if (group.Count() > 1)
            {
                var times = group.Select(g => g.TimeSlot.GetTimePattern()).Distinct().ToList();
                if (times.Count > 1)
                {
                    penalty += 20; // Prefer consistent times (e.g., always at 9:00-10:30)
                }
            }
        }

        // Soft Constraint 2: Respect preferred day patterns (MW, TTh, etc.)
        foreach (var gene in Genes)
        {
            bool matchesPattern = CheckDayPattern(gene, Genes);
            if (!matchesPattern)
                penalty += 10;
        }

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
                        penalty += 3;  // Small penalty for each gap
                    }
                }
            }
        }

        // Soft Constraint 4: Prefer morning classes (before 13:00)
        foreach (var gene in Genes)
        {
            if (string.Compare(gene.TimeSlot.StartTime, "13:00") >= 0)
                penalty += 2;
        }

        // Soft Constraint 5: Same section should have classes in nearby rooms
        foreach (var sectionGroup in sectionSchedule)
        {
            var rooms = sectionGroup.Select(g => g.Room.Id).Distinct().ToList();
            if (rooms.Count > 5)  // Too many different rooms
                penalty += 5;
        }

        // Normalized fitness (0-1000 range)
        double maxPenalty = CalculateMaxPossiblePenalty(data);
        _cachedFitness = 1000 * (1 - (penalty / maxPenalty));
        return Math.Max(_cachedFitness.Value, 0);
    }

    private bool CheckDayPattern(Gene gene, List<Gene> allGenes)
    {
        var subject = gene.Subject;
        var section = gene.Section;

        // Get all genes for same subject-section combination
        var relatedGenes = allGenes.Where(g =>
            g.Subject.Id == subject.Id &&
            g.Section.Id == section.Id).ToList();

        if (relatedGenes.Count <= 1)
            return true;  // Single session, no pattern to check

        var days = relatedGenes.Select(g => g.TimeSlot.Day).ToList();

        switch (subject.PreferredDayPattern)
        {
            case DayPattern.MW:
                return days.All(d => d == "Monday" || d == "Wednesday");

            case DayPattern.TTh:
                return days.All(d => d == "Tuesday" || d == "Thursday");

            case DayPattern.MWF:
                return days.All(d => d == "Monday" || d == "Wednesday" || d == "Friday");

            case DayPattern.Daily:
                return true;  // Any day is fine

            case DayPattern.Single:
                return days.Distinct().Count() == 1;

            default:
                return true;
        }
    }

    private double CalculateMaxPossiblePenalty(ScheduleData data)
    {
        int totalSessions = data.Sections.Sum(s =>
            s.SubjectIds.Sum(subId =>
                data.Subjects.First(sub => sub.Id == subId).DaysPerWeek));

        double maxPenalty = 0;
        maxPenalty += totalSessions * 100;   // Room capacity
        maxPenalty += totalSessions * 150;   // Room type compatibility
        maxPenalty += totalSessions * 200;   // Section conflicts
        maxPenalty += totalSessions * 100;   // Professor conflicts
        maxPenalty += totalSessions * 100;   // Room conflicts
        maxPenalty += totalSessions * 150;   // Professor qualification
        maxPenalty += totalSessions * 200;   // DaysPerWeek violation
        maxPenalty += totalSessions * 150;   // HoursPerDay mismatch
        maxPenalty += totalSessions * 20;    // Time consistency
        maxPenalty += totalSessions * 15;    // Time pattern consistency
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
        return new Schedule(newGenes);
    }

    public void Display()
    {
        Console.WriteLine($"{"Subject",-10} | {"Section",-12} | {"Room",-15} | {"Time Slot",-20} | Professor");
        Console.WriteLine(new string('-', 95));

        foreach (var gene in Genes.OrderBy(g => g.Section.Name)
                                  .ThenBy(g => g.TimeSlot.Day)
                                  .ThenBy(g => g.TimeSlot.StartTime))
        {
            Console.WriteLine(gene.ToString());
        }
    }

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