namespace Enhance_Genetic_Algorithm_v2.Models;
public class Professor
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public List<string> SubjectIds { get; set; }  // Subjects they can teach

    public Professor(string id, string name, string department)
    {
        Id = id;
        Name = name;
        Department = department;
        SubjectIds = new List<string>();
    }

    public override string ToString() => Name;
}