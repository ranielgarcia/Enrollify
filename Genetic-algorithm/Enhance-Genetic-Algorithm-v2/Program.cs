using Enhance_Genetic_Algorithm_v2;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║   Enhanced Course Scheduling System V3 - Genetic Algorithm      ║");
Console.WriteLine("║              Program | Section | Subject Architecture            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");

var data = new ScheduleData();

Console.WriteLine("═══ SYSTEM CONFIGURATION ═══");
Console.WriteLine($"Courses: {data.Courses.Count}");
foreach (var course in data.Courses)
{
    var sectionCount = data.Sections.Count(s => s.CourseId == course.Id);
    Console.WriteLine($"  • {course.Code}: {sectionCount} sections");
}

Console.WriteLine($"\nSections: {data.Sections.Count}");
Console.WriteLine($"Subjects: {data.Subjects.Count}");
Console.WriteLine($"Professors: {data.Professors.Count}");
Console.WriteLine($"Rooms: {data.Rooms.Count}");
Console.WriteLine($"Time Slots: {data.TimeSlots.Count}");
Console.WriteLine($"Total Classes: {data.Sections.Sum(s => s.SubjectIds.Count)}\n");

// Choose configuration
Console.WriteLine("Select Configuration:");
Console.WriteLine("1. Default (Balanced)");
Console.WriteLine("2. Fast Convergence");
Console.WriteLine("3. High Quality");
Console.WriteLine("4. Real World University");
Console.Write("\nChoice (1-3, default=1): ");

GAConfiguration config;
var choice = Console.ReadLine();
switch (choice)
{
    case "2":
        config = GAConfiguration.FastConvergence;
        Console.WriteLine("Using Fast Convergence configuration");
        break;
    case "3":
        config = GAConfiguration.HighQuality;
        Console.WriteLine("Using High Quality configuration");
        break;
    case "4":
        config = GAConfiguration.RealWorldUniversity;
        Console.WriteLine("Using High Quality configuration");
        break;
    default:
        config = GAConfiguration.Default;
        Console.WriteLine("Using Default configuration");
        break;
}
Console.WriteLine();

var ga = new GeneticAlgorithm(data, config);
var bestSchedule = ga.Run();

Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    OPTIMIZED SCHEDULE RESULT                     ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝\n");

// Display by section
bestSchedule.DisplayBySection(data);

Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine($"║ Fitness Score:        {bestSchedule.CalculateFitness(data),10:F2}                              ║");
Console.WriteLine($"║ Hard Violations:      {bestSchedule.GetViolationCount(data),3}                                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");

// Detect and display conflicts
var conflicts = ConflictDetector.DetectAllConflicts(bestSchedule, data);
if (conflicts.Count > 0)
{
    Console.WriteLine("\n⚠ CONFLICTS DETECTED:");
    foreach (var conflict in conflicts.Take(10))
    {
        Console.WriteLine($"  • {conflict}");
    }
    if (conflicts.Count > 10)
        Console.WriteLine($"  ... and {conflicts.Count - 10} more");
}
else
{
    Console.WriteLine("\n✓ No hard constraint violations detected!");
}

// Statistics
Console.WriteLine("\n═══ SCHEDULE STATISTICS ═══");

var professorLoad = bestSchedule.Genes.GroupBy(g => g.ProfessorId);
Console.WriteLine("\nProfessor Teaching Load:");
foreach (var profGroup in professorLoad.OrderByDescending(g => g.Count()))
{
    var prof = data.Professors.First(p => p.Id == profGroup.Key);
    var uniqueSubjects = profGroup.Select(g => g.Subject.Id).Distinct().Count();
    Console.WriteLine($"  {prof.Name,-20}: {profGroup.Count()} classes, {uniqueSubjects} subjects");
}

var roomUtilization = bestSchedule.Genes.GroupBy(g => g.Room.Id);
Console.WriteLine("\nTop 5 Most Used Rooms:");
foreach (var roomGroup in roomUtilization.OrderByDescending(g => g.Count()).Take(5))
{
    var room = data.Rooms.First(r => r.Id == roomGroup.Key);
    var utilizationRate = (roomGroup.Count() / (double)data.TimeSlots.Count) * 100;
    Console.WriteLine($"  {room.Name,-20}: {roomGroup.Count()} classes ({utilizationRate:F1}% utilization)");
}

var dayDistribution = bestSchedule.Genes.GroupBy(g => g.TimeSlot.Day);
Console.WriteLine("\nClasses per Day:");
foreach (var dayGroup in dayDistribution.OrderBy(g => g.Key))
{
    Console.WriteLine($"  {dayGroup.Key,-10}: {dayGroup.Count()} classes");
}

// Export options
Console.WriteLine("\n═══ EXPORT OPTIONS ═══");
Console.WriteLine("1. Export to CSV");
Console.WriteLine("2. Export by Section (Text)");
Console.WriteLine("3. Export by Professor (Text)");
Console.WriteLine("4. Export by Room (Text)");
Console.WriteLine("5. Export All");
Console.Write("\nExport choice (1-5, Enter to skip): ");

var exportChoice = Console.ReadLine();
if (!string.IsNullOrWhiteSpace(exportChoice))
{
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

    switch (exportChoice)
    {
        case "1":
            ScheduleExporter.ExportToCSV(bestSchedule, data, $"schedule_{timestamp}.csv");
            break;
        case "2":
            ScheduleExporter.ExportBySectionToText(bestSchedule, data, $"schedule_by_section_{timestamp}.txt");
            break;
        case "3":
            ScheduleExporter.ExportByProfessorToText(bestSchedule, data, $"schedule_by_professor_{timestamp}.txt");
            break;
        case "4":
            ScheduleExporter.ExportByRoomToText(bestSchedule, data, $"schedule_by_room_{timestamp}.txt");
            break;
        case "5":
            ScheduleExporter.ExportToCSV(bestSchedule, data, $"schedule_{timestamp}.csv");
            ScheduleExporter.ExportBySectionToText(bestSchedule, data, $"schedule_by_section_{timestamp}.txt");
            ScheduleExporter.ExportByProfessorToText(bestSchedule, data, $"schedule_by_professor_{timestamp}.txt");
            ScheduleExporter.ExportByRoomToText(bestSchedule, data, $"schedule_by_room_{timestamp}.txt");
            break;
    }
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();