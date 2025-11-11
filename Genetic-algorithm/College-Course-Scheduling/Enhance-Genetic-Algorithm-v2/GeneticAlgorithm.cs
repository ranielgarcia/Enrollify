using Enhance_Genetic_Algorithm_v2.Models;

namespace Enhance_Genetic_Algorithm_v2;
public class GeneticAlgorithm
{
    private readonly GAConfiguration _config;
    private readonly ScheduleData _data;
    private readonly Random _random;

    public GeneticAlgorithm(ScheduleData data, GAConfiguration config = null)
    {
        _data = data;
        _config = config ?? GAConfiguration.Default;
        _random = new Random();
    }

    public Schedule Run()
    {
        var population = InitializePopulation();
        Schedule bestSchedule = null;
        double bestFitness = 0;
        int generationsWithoutImprovement = 0;

        Console.WriteLine("Starting Enhanced Genetic Algorithm V3...");
        Console.WriteLine($"Configuration: Pop={_config.PopulationSize}, " +
                         $"MaxGen={_config.MaxGenerations}, " +
                         $"Crossover={_config.CrossoverRate:P0}, " +
                         $"Mutation={_config.MutationRate:P0}");
        Console.WriteLine($"Courses: {_data.Courses.Count}");
        Console.WriteLine($"Sections: {_data.Sections.Count}");
        Console.WriteLine($"Subjects: {_data.Subjects.Count}");
        Console.WriteLine($"Total Classes: {_data.Sections.Sum(s => s.SubjectIds.Count)}\n");

        for (int generation = 1; generation <= _config.MaxGenerations; generation++)
        {
            double previousBest = bestFitness;

            foreach (var schedule in population)
            {
                double fitness = schedule.CalculateFitness(_data);
                if (fitness > bestFitness)
                {
                    bestFitness = fitness;
                    bestSchedule = schedule.Clone();
                    generationsWithoutImprovement = 0;
                }
            }

            if (bestFitness == previousBest)
                generationsWithoutImprovement++;

            if (generation % 50 == 0 || generation == 1)
            {
                Console.WriteLine($"Gen {generation,4}: Fitness = {bestFitness:F2}, " +
                                $"Violations = {bestSchedule.GetViolationCount(_data)}, " +
                                $"No Improve = {generationsWithoutImprovement}");
            }

            if (bestFitness >= _config.TargetFitness)
            {
                Console.WriteLine($"\n✓ Target fitness reached at generation {generation}!");
                break;
            }

            // Adaptive mutation
            double adaptiveMutationRate = _config.MutationRate;
            if (_config.UseAdaptiveMutation && generationsWithoutImprovement > _config.AdaptiveMutationThreshold)
            {
                adaptiveMutationRate = Math.Min(0.4, _config.MutationRate * 1.5);
            }

            var newPopulation = new List<Schedule>();

            // Elitism: Keep best schedules
            var sortedPopulation = population.OrderByDescending(s => s.CalculateFitness(_data)).ToList();
            for (int i = 0; i < _config.EliteCount && i < sortedPopulation.Count; i++)
            {
                newPopulation.Add(sortedPopulation[i].Clone());
            }

            while (newPopulation.Count < _config.PopulationSize)
            {
                var parent1 = SelectParent(population);
                var parent2 = SelectParent(population);

                Schedule offspring;
                if (_random.NextDouble() < _config.CrossoverRate)
                    offspring = Crossover(parent1, parent2);
                else
                    offspring = parent1.Clone();

                if (_random.NextDouble() < adaptiveMutationRate)
                    Mutate(offspring);

                newPopulation.Add(offspring);
            }

            population = newPopulation;
        }

        Console.WriteLine($"\nCompleted: {_config.MaxGenerations} generations");
        return bestSchedule;
    }


    private List<Schedule> InitializePopulation()
    {
        var population = new List<Schedule>();

        for (int i = 0; i < _config.PopulationSize; i++)
        {
            var schedule = new Schedule(_config);

            foreach (var section in _data.Sections)
            {
                foreach (var subjectId in section.SubjectIds)
                {
                    var subject = _data.Subjects.First(s => s.Id == subjectId);

                    // Get compatible rooms
                    var compatibleRooms = _data.Rooms
                        .Where(r => r.IsCompatibleWith(subject) && r.Capacity >= section.StudentCount)
                        .ToList();

                    if (compatibleRooms.Count == 0)
                        compatibleRooms = _data.Rooms.Where(r => r.Capacity >= section.StudentCount).ToList();
                    if (compatibleRooms.Count == 0)
                        compatibleRooms = _data.Rooms;

                    // Get qualified professors
                    var qualifiedProfs = subject.ProfessorIds;
                    if (qualifiedProfs.Count == 0)
                    {
                        Console.WriteLine($"WARNING: No professors for {subject.Code}");
                        continue;
                    }

                    // NEW: Get timeslots that match the required HoursPerDay
                    var appropriateSlots = _data.TimeSlots
                        .Where(t => Math.Abs(t.GetDurationHours() - subject.HoursPerDay) < 0.1)
                        .ToList();

                    if (appropriateSlots.Count == 0)
                    {
                        Console.WriteLine($"WARNING: No timeslots match {subject.HoursPerDay} hours for {subject.Code}");
                        appropriateSlots = _data.TimeSlots;
                    }

                    // Filter by day pattern
                    var patternSlots = FilterSlotsByPattern(appropriateSlots, subject.DayAndTimePreference.PreferredDayPattern);

                    // NEW: Schedule for DaysPerWeek
                    var selectedDays = new HashSet<string>();
                    var professor = qualifiedProfs[_random.Next(qualifiedProfs.Count)];

                    for (int day = 0; day < subject.DayAndTimePreference.DaysPerWeek; day++)
                    {
                        // Find slots on days not yet used
                        var availableSlots = patternSlots
                            .Where(s => !selectedDays.Contains(s.Day))
                            .ToList();

                        if (availableSlots.Count == 0)
                            availableSlots = appropriateSlots.Where(s => !selectedDays.Contains(s.Day)).ToList();

                        if (availableSlots.Count == 0)
                            availableSlots = appropriateSlots; // Fallback

                        var slot = availableSlots[_random.Next(availableSlots.Count)];
                        selectedDays.Add(slot.Day);

                        var gene = new Gene(
                            subject,
                            section,
                            compatibleRooms[_random.Next(compatibleRooms.Count)],
                            slot,
                            professor
                        );
                        schedule.Genes.Add(gene);
                    }
                }
            }

            population.Add(schedule);
        }

        return population;
    }

    private List<TimeSlot> FilterSlotsByPattern(List<TimeSlot> slots, DayPattern pattern)
    {
        switch (pattern)
        {
            case DayPattern.MW:
                return slots.Where(t => t.Day == "Monday" || t.Day == "Wednesday").ToList();
            case DayPattern.TTh:
                return slots.Where(t => t.Day == "Tuesday" || t.Day == "Thursday").ToList();
            case DayPattern.MWF:
                return slots.Where(t => t.Day == "Monday" || t.Day == "Wednesday" || t.Day == "Friday").ToList();
            default:
                return slots;
        }
    }

    private List<TimeSlot> GetTimeSlotsForPattern(DayPattern pattern)
    {
        switch (pattern)
        {
            case DayPattern.MW:
                return _data.TimeSlots.Where(t => t.Day == "Monday" || t.Day == "Wednesday").ToList();

            case DayPattern.TTh:
                return _data.TimeSlots.Where(t => t.Day == "Tuesday" || t.Day == "Thursday").ToList();

            case DayPattern.MWF:
                return _data.TimeSlots.Where(t => t.Day == "Monday" || t.Day == "Wednesday" || t.Day == "Friday").ToList();

            case DayPattern.Single:
                // Group by time and pick random day
                var timePatterns = _data.TimeSlots.GroupBy(t => t.GetTimePattern()).ToList();
                var randomPattern = timePatterns[_random.Next(timePatterns.Count)];
                return randomPattern.Take(1).ToList();

            default:
                return _data.TimeSlots;
        }
    }

    private Schedule SelectParent(List<Schedule> population)
    {
        var tournament = new List<Schedule>();

        for (int i = 0; i < _config.TournamentSize; i++)
        {
            var randomSchedule = population[_random.Next(population.Count)];
            tournament.Add(randomSchedule);
        }

        return tournament.OrderByDescending(s => s.CalculateFitness(_data)).First();
    }

    private Schedule Crossover(Schedule parent1, Schedule parent2)
    {
        // Section-based crossover: take complete sections from each parent
        var offspring = new Schedule(_config);
        var sectionsUsed = new HashSet<string>();

        // Randomly decide which sections come from which parent
        foreach (var section in _data.Sections)
        {
            var useParent1 = _random.NextDouble() < 0.5;
            var parentGenes = useParent1
                ? parent1.Genes.Where(g => g.Section.Id == section.Id)
                : parent2.Genes.Where(g => g.Section.Id == section.Id);

            foreach (var gene in parentGenes)
            {
                offspring.Genes.Add(gene.Clone());
            }
        }

        return offspring;
    }

    private void Mutate(Schedule schedule)
    {
        int mutationCount = _random.Next(1, 4);  // 1-3 mutations

        for (int m = 0; m < mutationCount; m++)
        {
            if (schedule.Genes.Count == 0) break;

            int geneIndex = _random.Next(schedule.Genes.Count);
            var gene = schedule.Genes[geneIndex];

            int mutationType = _random.Next(4);

            switch (mutationType)
            {
                case 0: // Change room (must be compatible)
                    var compatibleRooms = _data.Rooms
                        .Where(r => r.IsCompatibleWith(gene.Subject) && r.Capacity >= gene.Section.StudentCount)
                        .ToList();

                    if (compatibleRooms.Count > 0)
                        gene.Room = compatibleRooms[_random.Next(compatibleRooms.Count)];
                    break;

                case 1: // Change timeslot (respect day pattern)
                    var appropriateSlots = GetTimeSlotsForPattern(gene.Subject.DayAndTimePreference.PreferredDayPattern);
                    if (appropriateSlots.Count > 0)
                        gene.TimeSlot = appropriateSlots[_random.Next(appropriateSlots.Count)];
                    break;

                case 2: // Change professor (must be qualified)
                    if (gene.Subject.ProfessorIds.Count > 1)
                    {
                        var otherProfs = gene.Subject.ProfessorIds.Where(p => p != gene.ProfessorId).ToList();
                        if (otherProfs.Count > 0)
                            gene.ProfessorId = otherProfs[_random.Next(otherProfs.Count)];
                    }
                    break;

                case 3: // Swap timeslots of two genes in same section
                    var sameSection = schedule.Genes
                        .Where(g => g.Section.Id == gene.Section.Id && g != gene)
                        .ToList();

                    if (sameSection.Count > 0)
                    {
                        var otherGene = sameSection[_random.Next(sameSection.Count)];
                        var tempSlot = gene.TimeSlot;
                        gene.TimeSlot = otherGene.TimeSlot;
                        otherGene.TimeSlot = tempSlot;
                    }
                    break;
            }
        }
    }
}