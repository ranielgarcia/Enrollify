using Enhance_Genetic_Algorithm.Models;

namespace Enhance_Genetic_Algorithm;
public class GeneticAlgorithm
{
    private const int POPULATION_SIZE = 150;  // Increased for complexity
    private const int MAX_GENERATIONS = 1000;
    private const double CROSSOVER_RATE = 0.8;
    private const double MUTATION_RATE = 0.15;  // Slightly higher for exploration
    private const int TOURNAMENT_SIZE = 5;
    private const double TARGET_FITNESS = 950;

    private readonly ScheduleData _data;
    private readonly Random _random;
    private List<Room> _compatibleRoomsCache;

    public GeneticAlgorithm(ScheduleData data)
    {
        _data = data;
        _random = new Random();
    }

    public Schedule Run()
    {
        var population = InitializePopulation();
        Schedule bestSchedule = null;
        double bestFitness = 0;

        Console.WriteLine("Starting Enhanced Genetic Algorithm...");
        Console.WriteLine($"Total Sessions to Schedule: {_data.Courses.Sum(c => c.SessionsPerWeek)}\n");

        for (int generation = 1; generation <= MAX_GENERATIONS; generation++)
        {
            foreach (var schedule in population)
            {
                double fitness = schedule.CalculateFitness(_data);
                if (fitness > bestFitness)
                {
                    bestFitness = fitness;
                    bestSchedule = schedule.Clone();
                }
            }

            if (generation % 50 == 0 || generation == 1)
            {
                Console.WriteLine($"Generation {generation,4}: Best Fitness = {bestFitness:F2}, " +
                                $"Violations = {bestSchedule.GetViolationCount(_data)}");
            }

            if (bestFitness >= TARGET_FITNESS)
            {
                Console.WriteLine($"\nTarget fitness reached at generation {generation}!");
                break;
            }

            var newPopulation = new List<Schedule>();

            while (newPopulation.Count < POPULATION_SIZE)
            {
                var parent1 = SelectParent(population);
                var parent2 = SelectParent(population);

                Schedule offspring;
                if (_random.NextDouble() < CROSSOVER_RATE)
                    offspring = Crossover(parent1, parent2);
                else
                    offspring = parent1.Clone();

                if (_random.NextDouble() < MUTATION_RATE)
                    Mutate(offspring);

                newPopulation.Add(offspring);
            }

            population = newPopulation;
        }

        return bestSchedule;
    }

    private List<Schedule> InitializePopulation()
    {
        var population = new List<Schedule>();

        for (int i = 0; i < POPULATION_SIZE; i++)
        {
            var schedule = new Schedule();

            // For each course, create multiple sessions based on SessionsPerWeek
            foreach (var course in _data.Courses)
            {
                for (int session = 1; session <= course.SessionsPerWeek; session++)
                {
                    // Get compatible rooms for this course
                    var compatibleRooms = _data.Rooms.Where(r => r.IsCompatibleWith(course)).ToList();

                    if (compatibleRooms.Count == 0)
                    {
                        Console.WriteLine($"WARNING: No compatible rooms for {course.Name}");
                        compatibleRooms = _data.Rooms; // Fallback
                    }

                    var gene = new Gene(
                        course,
                        compatibleRooms[_random.Next(compatibleRooms.Count)],
                        _data.TimeSlots[_random.Next(_data.TimeSlots.Count)],
                        session
                    );
                    schedule.Genes.Add(gene);
                }
            }

            population.Add(schedule);
        }

        return population;
    }

    private Schedule SelectParent(List<Schedule> population)
    {
        var tournament = new List<Schedule>();

        for (int i = 0; i < TOURNAMENT_SIZE; i++)
        {
            var randomSchedule = population[_random.Next(population.Count)];
            tournament.Add(randomSchedule);
        }

        return tournament.OrderByDescending(s => s.CalculateFitness(_data)).First();
    }

    private Schedule Crossover(Schedule parent1, Schedule parent2)
    {
        int crossoverPoint = _random.Next(1, parent1.Genes.Count);

        var offspringGenes = new List<Gene>();

        for (int i = 0; i < crossoverPoint; i++)
        {
            offspringGenes.Add(parent1.Genes[i].Clone());
        }

        for (int i = crossoverPoint; i < parent2.Genes.Count; i++)
        {
            offspringGenes.Add(parent2.Genes[i].Clone());
        }

        return new Schedule(offspringGenes);
    }

    private void Mutate(Schedule schedule)
    {
        int geneIndex = _random.Next(schedule.Genes.Count);
        var gene = schedule.Genes[geneIndex];

        int mutationType = _random.Next(3);

        switch (mutationType)
        {
            case 0: // Change room (must be compatible)
                var compatibleRooms = _data.Rooms.Where(r => r.IsCompatibleWith(gene.Course)).ToList();
                if (compatibleRooms.Count > 0)
                    gene.Room = compatibleRooms[_random.Next(compatibleRooms.Count)];
                break;

            case 1: // Change timeslot
                gene.TimeSlot = _data.TimeSlots[_random.Next(_data.TimeSlots.Count)];
                break;

            case 2: // Swap two genes (room and timeslot)
                int otherIndex = _random.Next(schedule.Genes.Count);

                var temp = schedule.Genes[geneIndex].Room;
                schedule.Genes[geneIndex].Room = schedule.Genes[otherIndex].Room;
                schedule.Genes[otherIndex].Room = temp;

                var tempSlot = schedule.Genes[geneIndex].TimeSlot;
                schedule.Genes[geneIndex].TimeSlot = schedule.Genes[otherIndex].TimeSlot;
                schedule.Genes[otherIndex].TimeSlot = tempSlot;
                break;
        }
    }
}