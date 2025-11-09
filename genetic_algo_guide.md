# Genetic Algorithm for Course Scheduling

## Core Concept

Genetic Algorithms (GA) mimic natural evolution to find optimal solutions. They work with a **population** of candidate solutions that evolve over generations through selection, crossover, and mutation.

## Key Terms

- **Chromosome**: A complete schedule (one possible solution)
- **Gene**: A single course assignment (course, room, timeslot, professor)
- **Fitness**: How good a schedule is (lower constraint violations = higher fitness)
- **Population**: Collection of candidate schedules
- **Generation**: One iteration of evolution

## How It Works

```
1. Create random initial population of schedules
2. Evaluate fitness of each schedule
3. While (not converged):
   a. Select best schedules (parents)
   b. Create offspring through crossover
   c. Apply random mutations
   d. Evaluate new population
   e. Replace weak schedules
4. Return best schedule found
```

## Sample Data

```csharp
// Courses
CS101: Data Structures (30 students)
CS201: Algorithms (25 students)
CS301: AI (20 students)
MATH101: Calculus (40 students)

// Rooms
R101: Capacity 35
R102: Capacity 30
R201: Capacity 45

// Timeslots
MON-9AM, MON-11AM, TUE-9AM, TUE-11AM, WED-9AM

// Professors
Prof. Smith (teaches CS101, CS201)
Prof. Jones (teaches CS301, MATH101)

// Constraints (Hard)
- No professor teaches two courses at same time
- Room capacity >= course students
- No room double-booked

// Constraints (Soft - for optimization)
- Minimize gaps in professor schedules
- Prefer morning slots
- Balance room usage
```

## Chromosome Representation

```
Gene = [CourseID, RoomID, TimeslotID, ProfessorID]

Chromosome (Complete Schedule):
[
  [CS101, R102, MON-9AM, Smith],
  [CS201, R101, TUE-11AM, Smith],
  [CS301, R102, WED-9AM, Jones],
  [MATH101, R201, MON-11AM, Jones]
]
```

## Pseudo Code

### Main Algorithm

```
FUNCTION GeneticAlgorithm():
    population = InitializePopulation(POPULATION_SIZE)
    
    FOR generation = 1 TO MAX_GENERATIONS:
        // Evaluate fitness
        FOR each chromosome IN population:
            chromosome.fitness = CalculateFitness(chromosome)
        
        // Check convergence
        IF BestFitness(population) >= TARGET_FITNESS:
            BREAK
        
        // Create new generation
        newPopulation = []
        
        WHILE newPopulation.size < POPULATION_SIZE:
            // Selection
            parent1 = SelectParent(population)
            parent2 = SelectParent(population)
            
            // Crossover
            IF Random() < CROSSOVER_RATE:
                offspring = Crossover(parent1, parent2)
            ELSE:
                offspring = parent1.Clone()
            
            // Mutation
            IF Random() < MUTATION_RATE:
                Mutate(offspring)
            
            newPopulation.Add(offspring)
        
        population = newPopulation
    
    RETURN BestChromosome(population)
```

### Fitness Function

```
FUNCTION CalculateFitness(chromosome):
    penalty = 0
    
    // Hard constraints (heavy penalties)
    FOR each gene IN chromosome:
        // Room capacity violation
        IF gene.room.capacity < gene.course.students:
            penalty += 100
        
        // Professor double-booking
        IF ProfessorHasConflict(gene, chromosome):
            penalty += 100
        
        // Room double-booking
        IF RoomHasConflict(gene, chromosome):
            penalty += 100
    
    // Soft constraints (light penalties)
    FOR each professor:
        gaps = CalculateScheduleGaps(professor, chromosome)
        penalty += gaps * 5
    
    FOR each gene IN chromosome:
        IF gene.timeslot NOT IN PreferredSlots:
            penalty += 2
    
    // Fitness is inverse of penalty
    fitness = 1000 - penalty
    RETURN MAX(fitness, 0)
```

### Selection (Tournament)

```
FUNCTION SelectParent(population):
    tournament = []
    
    FOR i = 1 TO TOURNAMENT_SIZE:
        randomChromosome = population[Random(0, population.size)]
        tournament.Add(randomChromosome)
    
    RETURN GetBest(tournament)  // Highest fitness
```

### Crossover (Single Point)

```
FUNCTION Crossover(parent1, parent2):
    crossoverPoint = Random(1, parent1.length - 1)
    
    offspring = []
    
    // Take genes from parent1 up to crossover point
    FOR i = 0 TO crossoverPoint:
        offspring.Add(parent1.genes[i])
    
    // Take remaining genes from parent2
    FOR i = crossoverPoint + 1 TO parent2.length:
        offspring.Add(parent2.genes[i])
    
    RETURN offspring
```

### Mutation

```
FUNCTION Mutate(chromosome):
    geneIndex = Random(0, chromosome.length)
    gene = chromosome.genes[geneIndex]
    
    mutationType = Random(1, 3)
    
    SWITCH mutationType:
        CASE 1:  // Change room
            gene.room = RandomRoom()
        CASE 2:  // Change timeslot
            gene.timeslot = RandomTimeslot()
        CASE 3:  // Swap two genes
            otherIndex = Random(0, chromosome.length)
            Swap(chromosome.genes[geneIndex], chromosome.genes[otherIndex])
```

### Initialization

```
FUNCTION InitializePopulation(size):
    population = []
    
    FOR i = 1 TO size:
        chromosome = []
        
        FOR each course IN courses:
            gene = [
                course,
                RandomRoom(),
                RandomTimeslot(),
                course.assignedProfessor
            ]
            chromosome.Add(gene)
        
        population.Add(chromosome)
    
    RETURN population
```

## Typical Parameters

```
POPULATION_SIZE = 100
MAX_GENERATIONS = 500
CROSSOVER_RATE = 0.8  // 80% chance
MUTATION_RATE = 0.1   // 10% chance
TOURNAMENT_SIZE = 5
TARGET_FITNESS = 950  // Near-perfect schedule
```

## Why GA Works for Scheduling

✓ Handles both hard and soft constraints
✓ Finds "good enough" solutions quickly
✓ Doesn't get stuck in local optima (mutation helps)
✓ Easy to add new constraints via fitness function
✓ Scales reasonably well to large problems

## Typical Results

- Generation 1: Many constraint violations (fitness ~300)
- Generation 50: Most hard constraints met (fitness ~700)
- Generation 200: Near-optimal solution (fitness ~920)
- Generation 500: Best achievable (fitness ~950)

Not all constraints may be perfectly satisfied, but GA finds practical solutions balancing all requirements.