namespace Genetic_Algorithms.Models;
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

    // Calculate fitness score (higher is better)
    public double CalculateFitness(ScheduleData data)
    {
        if (_cachedFitness.HasValue)
            return _cachedFitness.Value;

        double penalty = 0;

        // Hard Constraint 1: Room capacity violations (heavy penalty)
        foreach (var gene in Genes)
        {
            if (gene.Room.Capacity < gene.Course.StudentCount)
                penalty += 100;
        }

        // Hard Constraint 2: Professor conflicts (teaching two courses at same time)
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

        // Hard Constraint 3: Room conflicts (same room at same time)
        var roomSchedule = Genes.GroupBy(g => new { roomId = g.Room.Id, timeSlotId = g.TimeSlot.Id });
        foreach (var group in roomSchedule)
        {
            if (group.Count() > 1)
                penalty += 100 * (group.Count() - 1);
        }

        // Soft Constraint 1: Minimize gaps in professor schedules
        foreach (var profGroup in professorSchedule)
        {
            var slots = profGroup.Select(g => g.TimeSlot.Id).OrderBy(s => s).ToList();
            if (slots.Count > 1)
            {
                // Simple gap detection - could be improved
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

        // Calculate max possible penalty
        double maxPenalty = data.Courses.Count * 300.0;

        // Normalize: 0 penalty = 1000 fitness, maxPenalty = 0 fitness
        _cachedFitness = 1000 * (1 - (penalty / maxPenalty));
        return Math.Max(_cachedFitness.Value, 0);
    }

    public int GetViolationCount(ScheduleData data)
    {
        int violations = 0;

        foreach (var gene in Genes)
        {
            if (gene.Room.Capacity < gene.Course.StudentCount)
                violations++;
        }

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

        var roomSchedule = Genes.GroupBy(g => new { roomId = g.Room.Id, timeSlotId = g.TimeSlot.Id });
        foreach (var group in roomSchedule)
        {
            if (group.Count() > 1)
                violations += group.Count() - 1;
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
        Console.WriteLine($"{"Course",-20} | {"Room",-10} | {"Time Slot",-12} | Professor");
        Console.WriteLine(new string('-', 70));
        foreach (var gene in Genes.OrderBy(g => g.TimeSlot.Id))
        {
            Console.WriteLine(gene.ToString());
        }
    }
}