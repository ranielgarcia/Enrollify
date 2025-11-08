using Enhance_Genetic_Algorithm;

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     Enhanced College Course Scheduling - Genetic Algorithm     ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

var data = new ScheduleData();

Console.WriteLine($"Courses: {data.Courses.Count}");
Console.WriteLine($"Total Sessions: {data.Courses.Sum(c => c.SessionsPerWeek)}");
Console.WriteLine($"Rooms: {data.Rooms.Count}");
Console.WriteLine($"Time Slots: {data.TimeSlots.Count}");
Console.WriteLine($"Professors: {data.Professors.Count}\n");

var ga = new GeneticAlgorithm(data);
var bestSchedule = ga.Run();

Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                      BEST SCHEDULE FOUND                         ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");

bestSchedule.Display();

Console.WriteLine($"\n╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║ Fitness Score: {bestSchedule.CalculateFitness(data),10:F2}       ║");
Console.WriteLine($"║ Constraint Violations: {bestSchedule.GetViolationCount(data),3}  ║");
Console.WriteLine($"╚══════════════════════════════════════════════════════════════════╝");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();