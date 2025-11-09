namespace Enhance_Genetic_Algorithm_v2.Models;
public class Subject
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public SubjectType Type { get; set; }
    public int Units { get; set; }
    public double HoursPerDay { get; set; }     // NEW: Hours per session (1.5, 2, 3, etc.)
    public List<string> ProfessorIds { get; set; }
    public bool RequiresLab { get; set; }
    public DayAndTimePreference DayAndTimePreference { get; set; }

    public Subject(string id, string code, string name, SubjectType type,
                  int units, double hoursPerDay, DayAndTimePreference dayAndTimePreference)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        Units = units;
        HoursPerDay = hoursPerDay;
        ProfessorIds = new List<string>();
        RequiresLab = false;
        DayAndTimePreference = dayAndTimePreference ?? new DayAndTimePreference(DayPattern.Single);
    }

    public override string ToString() => $"{Code} - {Name}";
}