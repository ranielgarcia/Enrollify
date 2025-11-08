namespace Genetic_Algorithms.Models;
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