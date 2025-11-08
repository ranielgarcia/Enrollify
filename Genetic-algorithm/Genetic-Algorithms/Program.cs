using Genetic_Algorithms;

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