namespace Enhance_Genetic_Algorithm.Models;
public class Course
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int StudentCount { get; set; }
    public string ProfessorId { get; set; }
    public CourseType Type { get; set; }
    public int SessionsPerWeek { get; set; }  // NEW: How many times per week

    public Course(string id, string name, int studentCount, string professorId,
                 CourseType type, int sessionsPerWeek = 1)
    {
        Id = id;
        Name = name;
        StudentCount = studentCount;
        ProfessorId = professorId;
        Type = type;
        SessionsPerWeek = sessionsPerWeek;
    }

    public override string ToString() => $"{Id} - {Name}";
}