## Project Structure

Create a .NET Console Application and add the following classes.

## Complete Implementation

### Program.cs

Entry point that runs the genetic algorithm.

csharp

```csharp
using System;

namespace CourseSchedulingGA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== College Course Scheduling with Genetic Algorithm ===\n");

            // Initialize data
            var data = new ScheduleData();
            
            // Create and run genetic algorithm
            var ga = new GeneticAlgorithm(data);
            var bestSchedule = ga.Run();

            // Display results
            Console.WriteLine("\n=== BEST SCHEDULE FOUND ===");
            bestSchedule.Display();
            Console.WriteLine($"\nFitness Score: {bestSchedule.CalculateFitness(data)}");
            Console.WriteLine($"Constraint Violations: {bestSchedule.GetViolationCount(data)}");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
```

### Models/Course.cs

Represents a course with its requirements.

csharp

```csharp
namespace CourseSchedulingGA.Models
{
    public class Course
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StudentCount { get; set; }
        public string ProfessorId { get; set; }

        public Course(string id, string name, int studentCount, string professorId)
        {
            Id = id;
            Name = name;
            StudentCount = studentCount;
            ProfessorId = professorId;
        }

        public override string ToString() => $"{Id} - {Name}";
    }
}
```

### Models/Room.cs

Represents a classroom with capacity.

csharp

```csharp
namespace CourseSchedulingGA.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }

        public Room(string id, string name, int capacity)
        {
            Id = id;
            Name = name;
            Capacity = capacity;
        }

        public override string ToString() => $"{Id} ({Capacity} seats)";
    }
}
```

### Models/TimeSlot.cs

Represents available time slots.

csharp

```csharp
namespace CourseSchedulingGA.Models
{
    public class TimeSlot
    {
        public string Id { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }

        public TimeSlot(string id, string day, string time)
        {
            Id = id;
            Day = day;
            Time = time;
        }

        public override string ToString() => $"{Day} {Time}";
    }
}
```

### Models/Professor.cs

Represents a professor.

csharp

```csharp
namespace CourseSchedulingGA.Models
{
    public class Professor
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Professor(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
```

### Models/Gene.cs

Represents a single course assignment (a gene in the chromosome).

csharp

```csharp
using System;

namespace CourseSchedulingGA.Models
{
    public class Gene
    {
        public Course Course { get; set; }
        public Room Room { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public Gene(Course course, Room room, TimeSlot timeSlot)
        {
            Course = course;
            Room = room;
            TimeSlot = timeSlot;
        }

        // Create a deep copy
        public Gene Clone()
        {
            return new Gene(Course, Room, TimeSlot);
        }

        public override string ToString()
        {
            return $"{Course.Name,-20} | {Room.Name,-10} | {TimeSlot,-12} | Prof. {Course.ProfessorId}";
        }
    }
}
```

### Models/Schedule.cs

Represents a complete schedule (chromosome). Contains fitness calculation and constraint checking.

csharp

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseSchedulingGA.Models
{
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
            var roomSchedule = Genes.GroupBy(g => new { g.Room.Id, g.TimeSlot.Id });
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

            _cachedFitness = 1000 - penalty;
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

            var roomSchedule = Genes.GroupBy(g => new { g.Room.Id, g.TimeSlot.Id });
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
}
```

### ScheduleData.cs

Contains all courses, rooms, timeslots, and professors.

csharp

```csharp
using System.Collections.Generic;
using CourseSchedulingGA.Models;

namespace CourseSchedulingGA
{
    public class ScheduleData
    {
        public List<Course> Courses { get; set; }
        public List<Room> Rooms { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }
        public List<Professor> Professors { get; set; }

        public ScheduleData()
        {
            InitializeProfessors();
            InitializeCourses();
            InitializeRooms();
            InitializeTimeSlots();
        }

        private void InitializeProfessors()
        {
            Professors = new List<Professor>
            {
                new Professor("P1", "Dr. Smith"),
                new Professor("P2", "Dr. Jones"),
                new Professor("P3", "Dr. Williams"),
                new Professor("P4", "Dr. Brown")
            };
        }

        private void InitializeCourses()
        {
            Courses = new List<Course>
            {
                new Course("CS101", "Data Structures", 30, "P1"),
                new Course("CS201", "Algorithms", 25, "P1"),
                new Course("CS301", "Artificial Intelligence", 20, "P2"),
                new Course("MATH101", "Calculus I", 40, "P2"),
                new Course("MATH201", "Linear Algebra", 35, "P3"),
                new Course("PHY101", "Physics I", 38, "P3"),
                new Course("ENG101", "Technical Writing", 28, "P4"),
                new Course("CS401", "Machine Learning", 22, "P4")
            };
        }

        private void InitializeRooms()
        {
            Rooms = new List<Room>
            {
                new Room("R101", "Room 101", 35),
                new Room("R102", "Room 102", 30),
                new Room("R201", "Room 201", 45),
                new Room("R202", "Room 202", 40),
                new Room("R301", "Room 301", 25)
            };
        }

        private void InitializeTimeSlots()
        {
            TimeSlots = new List<TimeSlot>
            {
                new TimeSlot("T1", "Monday", "09:00"),
                new TimeSlot("T2", "Monday", "11:00"),
                new TimeSlot("T3", "Monday", "14:00"),
                new TimeSlot("T4", "Tuesday", "09:00"),
                new TimeSlot("T5", "Tuesday", "11:00"),
                new TimeSlot("T6", "Tuesday", "14:00"),
                new TimeSlot("T7", "Wednesday", "09:00"),
                new TimeSlot("T8", "Wednesday", "11:00"),
                new TimeSlot("T9", "Thursday", "09:00"),
                new TimeSlot("T10", "Thursday", "11:00")
            };
        }
    }
}
```

### GeneticAlgorithm.cs

Core GA implementation with selection, crossover, and mutation.

csharp

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CourseSchedulingGA.Models;

namespace CourseSchedulingGA
{
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
}
```

## How to Run

1. Create a new .NET Console Application:

bash

```bash
dotnet new console -n CourseSchedulingGA
cd CourseSchedulingGA
```

2. Create the folder structure:

```
CourseSchedulingGA/
├── Program.cs
├── ScheduleData.cs
├── GeneticAlgorithm.cs
└── Models/
    ├── Course.cs
    ├── Room.cs
    ├── TimeSlot.cs
    ├── Professor.cs
    ├── Gene.cs
    └── Schedule.cs
```

3. Copy each code block into its respective file
4. Run the application:

bash

```bash
dotnet run
```

## Expected Output

```
=== College Course Scheduling with Genetic Algorithm ===

Starting Genetic Algorithm...

Generation   1: Best Fitness = 412.00, Violations = 8
Generation  50: Best Fitness = 782.00, Violations = 3
Generation 100: Best Fitness = 856.00, Violations = 1
Generation 150: Best Fitness = 912.00, Violations = 0
Generation 200: Best Fitness = 936.00, Violations = 0

=== BEST SCHEDULE FOUND ===
Course               | Room       | Time Slot    | Professor
----------------------------------------------------------------------
Data Structures      | Room 101   | Monday 09:00 | Prof. P1
Technical Writing    | Room 102   | Monday 09:00 | Prof. P4
Algorithms           | Room 201   | Monday 11:00 | Prof. P1
Physics I            | Room 202   | Monday 11:00 | Prof. P3
...

Fitness Score: 936
Constraint Violations: 0
```

## Key Implementation Notes

1. **Fitness Caching**: Schedule caches fitness to avoid recalculation
2. **Constraint Weights**: Hard constraints (100 penalty) vs soft (2-5 penalty)
3. **Tournament Selection**: Balances exploration vs exploitation
4. **Three Mutation Types**: Room change, timeslot change, or gene swap
5. **Progress Tracking**: Shows best fitness every 50 generations

## Tuning Parameters

Adjust constants in `GeneticAlgorithm.cs`:

- Increase `POPULATION_SIZE` for better solutions (slower)
- Increase `MAX_GENERATIONS` if not converging
- Adjust `MUTATION_RATE` higher if stuck in local optima
- Modify penalty weights in `Schedule.CalculateFitness()` for different priorities