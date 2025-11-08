using Enhance_Genetic_Algorithm_v2.Models;
using System.Text;

namespace Enhance_Genetic_Algorithm_v2;
public class ScheduleExporter
{
    public static void ExportToCSV(Schedule schedule, ScheduleData data, string filename)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Section,Subject Code,Subject Name,Days Per Week, Hours Per Day,Room,Day,Start Time,End Time, Hours,Professor,Students");

        foreach (var gene in schedule.Genes.OrderBy(g => g.Section.Name)
                                          .ThenBy(g => g.TimeSlot.Day)
                                          .ThenBy(g => g.TimeSlot.StartTime))
        {
            var selectedSlotNumberOfHours = CalculateHours(gene.TimeSlot.StartTime, gene.TimeSlot.EndTime);

            var professor = data.Professors.FirstOrDefault(p => p.Id == gene.ProfessorId);
            csv.AppendLine($"{gene.Section.Name},{gene.Subject.Code},{gene.Subject.Name},{gene.Subject.DaysPerWeek},{gene.Subject.HoursPerDay}," +
                          $"{gene.Room.Name},{gene.TimeSlot.Day},{gene.TimeSlot.StartTime}," +
                          $"{gene.TimeSlot.EndTime}, {selectedSlotNumberOfHours},{professor?.Name ?? gene.ProfessorId}," +
                          $"{gene.Section.StudentCount}");
        }

        File.WriteAllText(filename, csv.ToString());
        Console.WriteLine($"\n✓ Schedule exported to {filename}");
    }

    private static double CalculateHours(string startTime, string endTime)
    {
        TimeSpan start = TimeSpan.Parse(startTime);
        TimeSpan end = TimeSpan.Parse(endTime);

        return (end - start).TotalHours;
    }

    public static void ExportBySectionToText(Schedule schedule, ScheduleData data, string filename)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("              COLLEGE COURSE SCHEDULE");
        sb.AppendLine($"              Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════\n");

        var sections = schedule.Genes.GroupBy(g => g.Section.Id).OrderBy(g => g.Key);

        foreach (var sectionGroup in sections)
        {
            var section = data.Sections.First(s => s.Id == sectionGroup.Key);
            var course = data.Courses.First(p => p.Id == section.CourseId); // TODO: rename ProgramId to CourseId

            sb.AppendLine($"█ {section.Name}");
            sb.AppendLine($"  Course: {course.Name}");
            sb.AppendLine($"  Year Level: {section.YearLevel} | Semester: {section.Semester} | Students: {section.StudentCount}");
            sb.AppendLine("  ─────────────────────────────────────────────────────────────");

            var dayGroups = sectionGroup.OrderBy(g => g.TimeSlot.Day).GroupBy(g => g.TimeSlot.Day);

            foreach (var dayGroup in dayGroups)
            {
                sb.AppendLine($"\n  {dayGroup.Key}:");
                foreach (var gene in dayGroup.OrderBy(g => g.TimeSlot.StartTime))
                {
                    var professor = data.Professors.FirstOrDefault(p => p.Id == gene.ProfessorId);
                    sb.AppendLine($"    {gene.TimeSlot.StartTime}-{gene.TimeSlot.EndTime}  " +
                                $"{gene.Subject.Code,-10} {gene.Subject.Name,-30} " +
                                $"| {gene.Room.Name,-12} | {professor?.Name ?? gene.ProfessorId}");
                }
            }

            sb.AppendLine("\n");
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"Fitness Score: {schedule.CalculateFitness(data):F2}");
        sb.AppendLine($"Violations: {schedule.GetViolationCount(data)}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        File.WriteAllText(filename, sb.ToString());
        Console.WriteLine($"✓ Schedule exported to {filename}");
    }

    public static void ExportByProfessorToText(Schedule schedule, ScheduleData data, string filename)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("         PROFESSOR TEACHING SCHEDULES");
        sb.AppendLine($"         Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════\n");

        var professorGroups = schedule.Genes.GroupBy(g => g.ProfessorId).OrderBy(g => g.Key);

        foreach (var profGroup in professorGroups)
        {
            var professor = data.Professors.FirstOrDefault(p => p.Id == profGroup.Key);
            sb.AppendLine($"█ {professor?.Name ?? profGroup.Key}");
            sb.AppendLine($"  Department: {professor?.Department ?? "N/A"}");
            sb.AppendLine($"  Total Classes: {profGroup.Count()}");
            sb.AppendLine("  ─────────────────────────────────────────────────────────────");

            var dayGroups = profGroup.OrderBy(g => g.TimeSlot.Day).GroupBy(g => g.TimeSlot.Day);

            foreach (var dayGroup in dayGroups)
            {
                sb.AppendLine($"\n  {dayGroup.Key}:");
                foreach (var gene in dayGroup.OrderBy(g => g.TimeSlot.StartTime))
                {
                    sb.AppendLine($"    {gene.TimeSlot.StartTime}-{gene.TimeSlot.EndTime}  " +
                                $"{gene.Subject.Code,-10} {gene.Section.Name,-12} " +
                                $"| {gene.Room.Name}");
                }
            }

            sb.AppendLine("\n");
        }

        File.WriteAllText(filename, sb.ToString());
        Console.WriteLine($"✓ Professor schedules exported to {filename}");
    }

    public static void ExportByRoomToText(Schedule schedule, ScheduleData data, string filename)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("            ROOM UTILIZATION SCHEDULE");
        sb.AppendLine($"            Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════\n");

        var roomGroups = schedule.Genes.GroupBy(g => g.Room.Id).OrderBy(g => g.Key);

        foreach (var roomGroup in roomGroups)
        {
            var room = data.Rooms.First(r => r.Id == roomGroup.Key);
            sb.AppendLine($"█ {room.Name}");
            sb.AppendLine($"  Type: {room.Type} | Capacity: {room.Capacity}");
            sb.AppendLine($"  Utilization: {roomGroup.Count()} classes scheduled");
            sb.AppendLine("  ─────────────────────────────────────────────────────────────");

            var dayGroups = roomGroup.OrderBy(g => g.TimeSlot.Day).GroupBy(g => g.TimeSlot.Day);

            foreach (var dayGroup in dayGroups)
            {
                sb.AppendLine($"\n  {dayGroup.Key}:");
                foreach (var gene in dayGroup.OrderBy(g => g.TimeSlot.StartTime))
                {
                    var professor = data.Professors.FirstOrDefault(p => p.Id == gene.ProfessorId);
                    sb.AppendLine($"    {gene.TimeSlot.StartTime}-{gene.TimeSlot.EndTime}  " +
                                $"{gene.Subject.Code,-10} {gene.Section.Name,-12} " +
                                $"| {professor?.Name ?? gene.ProfessorId}");
                }
            }

            sb.AppendLine("\n");
        }

        File.WriteAllText(filename, sb.ToString());
        Console.WriteLine($"✓ Room schedules exported to {filename}");
    }
}