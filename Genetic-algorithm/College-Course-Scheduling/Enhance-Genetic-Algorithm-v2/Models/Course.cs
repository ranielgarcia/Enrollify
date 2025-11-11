namespace Enhance_Genetic_Algorithm_v2.Models;
public class Course
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Department { get; set; }

    public Course(string id, string name, string code, string department)
    {
        Id = id;
        Name = name;
        Code = code;
        Department = department;
    }

    public override string ToString() => $"{Code} - {Name}";
}
