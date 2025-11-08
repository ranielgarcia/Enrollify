using Genetic_Algorithms.Models;

namespace Genetic_Algorithms;
public class GeneticAlgorithm
{
    private const int POPULATION_SIZE = 100;
    private const int MAX_GENERATIONS = 500;
    private const double CROSSOVER_RATE = 0.8;
    private const double MUTATION_RATE = 0.1;
    private const int TOURNAMENT_SIZE = 5;
    private const double TARGET_FITNESS = 950;

    private readonly ScheduleData _data;
    private readonly Random _random;

    public GeneticAlgorithm(ScheduleData data)
    {
        _data = data;
        _random = new Random();
    }

    // Main GA loop
    public Schedule Run()
    {
        var population = InitializePopulation();
        Schedule bestSchedule = null;
        double bestFitness = 0;

        Console.WriteLine("Starting Genetic Algorithm...\n");

        for (int generation = 1; generation <= MAX_GENERATIONS; generation++)
        {
            // Evaluate fitness for all schedules
            foreach (var schedule in population)
            {
                double fitness = schedule.CalculateFitness(_data);
                if (fitness > bestFitness)
                {
                    bestFitness = fitness;
                    bestSchedule = schedule.Clone();
                }
            }

            // Display progress every 50 generations
            if (generation % 50 == 0 || generation == 1)
            {
                Console.WriteLine($"Generation {generation,3}: Best Fitness = {bestFitness:F2}, " +
                                $"Violations = {bestSchedule.GetViolationCount(_data)}");
            }

            // Check if target reached
            if (bestFitness >= TARGET_FITNESS)
            {
                Console.WriteLine($"\nTarget fitness reached at generation {generation}!");
                break;
            }

            // Create new generation
            var newPopulation = new List<Schedule>();

            while (newPopulation.Count < POPULATION_SIZE)
            {
                // Selection
                var parent1 = SelectParent(population);
                var parent2 = SelectParent(population);

                // Crossover
                Schedule offspring;
                if (_random.NextDouble() < CROSSOVER_RATE)
                    offspring = Crossover(parent1, parent2);
                else
                    offspring = parent1.Clone();

                // Mutation
                if (_random.NextDouble() < MUTATION_RATE)
                    Mutate(offspring);

                newPopulation.Add(offspring);
            }

            population = newPopulation;
        }

        return bestSchedule;
    }

    // Create initial random population
    private List<Schedule> InitializePopulation()
    {
        var population = new List<Schedule>();

        for (int i = 0; i < POPULATION_SIZE; i++)
        {
            var schedule = new Schedule();

            foreach (var course in _data.Courses)
            {
                var gene = new Gene(
                    course,
                    _data.Rooms[_random.Next(_data.Rooms.Count)],
                    _data.TimeSlots[_random.Next(_data.TimeSlots.Count)]
                );
                schedule.Genes.Add(gene);
            }

            population.Add(schedule);
        }

        return population;
    }

    // Tournament selection - select best from random subset
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

    // Single-point crossover
    private Schedule Crossover(Schedule parent1, Schedule parent2)
    {
        int crossoverPoint = _random.Next(1, parent1.Genes.Count);

        var offspringGenes = new List<Gene>();

        // Take genes from parent1 up to crossover point
        for (int i = 0; i < crossoverPoint; i++)
        {
            offspringGenes.Add(parent1.Genes[i].Clone());
        }

        // Take remaining genes from parent2
        for (int i = crossoverPoint; i < parent2.Genes.Count; i++)
        {
            offspringGenes.Add(parent2.Genes[i].Clone());
        }

        return new Schedule(offspringGenes);
    }

    // Random mutation - change room or timeslot
    private void Mutate(Schedule schedule)
    {
        int geneIndex = _random.Next(schedule.Genes.Count);
        var gene = schedule.Genes[geneIndex];

        int mutationType = _random.Next(3);

        switch (mutationType)
        {
            case 0: // Change room
                gene.Room = _data.Rooms[_random.Next(_data.Rooms.Count)];
                break;
            case 1: // Change timeslot
                gene.TimeSlot = _data.TimeSlots[_random.Next(_data.TimeSlots.Count)];
                break;
            case 2: // Swap two genes
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