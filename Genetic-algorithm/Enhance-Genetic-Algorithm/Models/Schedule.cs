namespace Enhance_Genetic_Algorithm.Models;
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

    // Calculate fitness score with NORMALIZED scoring (0-1000)
    public double CalculateFitness(ScheduleData data)
    {
        if (_cachedFitness.HasValue)
            return _cachedFitness.Value;

        double penalty = 0;

        // Hard Constraint 1: Room capacity violations
        foreach (var gene in Genes)
        {
            if (gene.Room.Capacity < gene.Course.StudentCount)
                penalty += 100;
        }

        // Hard Constraint 2: Room type compatibility (NEW)
        foreach (var gene in Genes)
        {
            if (!gene.Room.IsCompatibleWith(gene.Course))
                penalty += 150;  // Heavy penalty for wrong room type
        }

        // Hard Constraint 3: Professor conflicts (teaching two courses at same time)
        var professorSchedule = Genes.GroupBy(g => g.Course.ProfessorId);
        foreach (var profGroup in professorSchedule)
        {
            var timeSlotGroups = profGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeSlotGroups)
            {
                if (timeGroup.Count() > 1)
                    penalty += 100 * (timeGroup.Count() - 1);
            }
        }

        // Hard Constraint 4: Room conflicts (same room at same time)
        var roomSchedule = Genes.GroupBy(g => new { roomId = g.Room.Id, timeSlotId = g.TimeSlot.Id });
        foreach (var group in roomSchedule)
        {
            if (group.Count() > 1)
                penalty += 100 * (group.Count() - 1);
        }

        // Hard Constraint 5: Multiple sessions for same course should be on different days (NEW)
        var courseSchedule = Genes.GroupBy(g => g.Course.Id);
        foreach (var courseGroup in courseSchedule)
        {
            var dayGroups = courseGroup.GroupBy(g => g.TimeSlot.Day);
            foreach (var dayGroup in dayGroups)
            {
                if (dayGroup.Count() > 1)
                    penalty += 80 * (dayGroup.Count() - 1);  // Prefer different days
            }
        }

        // Soft Constraint 1: Minimize gaps in professor schedules
        foreach (var profGroup in professorSchedule)
        {
            var slots = profGroup.Select(g => g.TimeSlot.Id).OrderBy(s => s).ToList();
            if (slots.Count > 1)
            {
                int gaps = slots.Count - 1;
                penalty += gaps * 5;
            }
        }

        // Soft Constraint 2: Prefer morning slots (before "13:00")
        foreach (var gene in Genes)
        {
            if (gene.TimeSlot.Time.CompareTo("13:00") >= 0)
                penalty += 2;
        }

        // Soft Constraint 3: Sessions of same course should be evenly spaced (NEW)
        foreach (var courseGroup in courseSchedule)
        {
            if (courseGroup.Count() > 1)
            {
                var days = courseGroup.Select(g => g.TimeSlot.Day).Distinct().ToList();
                if (days.Count < courseGroup.Count())
                    penalty += 10;  // Prefer spreading across different days
            }
        }

        // NORMALIZED FITNESS CALCULATION (Solution 2)
        // Calculate max possible penalty based on data size
        double maxPenalty = CalculateMaxPossiblePenalty(data);

        // Normalize to 0-1000 range
        _cachedFitness = 1000 * (1 - (penalty / maxPenalty));
        return Math.Max(_cachedFitness.Value, 0);
    }

    private double CalculateMaxPossiblePenalty(ScheduleData data)
    {
        // Estimate worst-case scenario penalties
        int totalSessions = data.Courses.Sum(c => c.SessionsPerWeek);

        double maxPenalty = 0;
        maxPenalty += totalSessions * 100;  // Room capacity violations
        maxPenalty += totalSessions * 150;  // Room type incompatibility
        maxPenalty += totalSessions * 100;  // Professor conflicts
        maxPenalty += totalSessions * 100;  // Room conflicts
        maxPenalty += totalSessions * 80;   // Same course same day
        maxPenalty += totalSessions * 5;    // Professor gaps
        maxPenalty += totalSessions * 2;    // Afternoon preference
        maxPenalty += data.Courses.Count * 10;  // Course spacing

        return maxPenalty;
    }

    public int GetViolationCount(ScheduleData data)
    {
        int violations = 0;

        // Room capacity
        foreach (var gene in Genes)
        {
            if (gene.Room.Capacity < gene.Course.StudentCount)
                violations++;
        }

        // Room type compatibility (NEW)
        foreach (var gene in Genes)
        {
            if (!gene.Room.IsCompatibleWith(gene.Course))
                violations++;
        }

        // Professor conflicts
        var professorSchedule = Genes.GroupBy(g => g.Course.ProfessorId);
        foreach (var profGroup in professorSchedule)
        {
            var timeSlotGroups = profGroup.GroupBy(g => g.TimeSlot.Id);
            foreach (var timeGroup in timeSlotGroups)
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

        // Same course same day (NEW)
        var courseSchedule = Genes.GroupBy(g => g.Course.Id);
        foreach (var courseGroup in courseSchedule)
        {
            var dayGroups = courseGroup.GroupBy(g => g.TimeSlot.Day);
            foreach (var dayGroup in dayGroups)
            {
                if (dayGroup.Count() > 1)
                    violations += dayGroup.Count() - 1;
            }
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
        Console.WriteLine($"{"Course",-30} | {"Room",-15} | {"Time Slot",-15} | Professor");
        Console.WriteLine(new string('-', 85));
        foreach (var gene in Genes.OrderBy(g => g.TimeSlot.Day)
                                  .ThenBy(g => g.TimeSlot.Time)
                                  .ThenBy(g => g.Course.Name))
        {
            Console.WriteLine(gene.ToString());
        }
    }
}