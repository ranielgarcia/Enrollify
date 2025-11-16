namespace Enhance_Genetic_Algorithm_v2.Models;
public class Professor
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public List<string> SubjectIds { get; set; }  // Subjects they can teach

    public int MaxTimeSlotsPerDay { get; set; }

    public Professor(string id, string name, string department, int maxTimeSlotsPerDay = 6)
    {
        Id = id;
        Name = name;
        Department = department;
        SubjectIds = new List<string>();
        MaxTimeSlotsPerDay = maxTimeSlotsPerDay;
    }

    public override string ToString() => Name;
}