namespace Enhance_Genetic_Algorithm_v2.Models;
public class Subject
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public SubjectType Type { get; set; }
    public int Units { get; set; }
    public int DaysPerWeek { get; set; }        // NEW: How many days per week (1-5)
    public double HoursPerDay { get; set; }     // NEW: Hours per session (1.5, 2, 3, etc.)
    public List<string> ProfessorIds { get; set; }
    public DayPattern PreferredDayPattern { get; set; }
    public bool RequiresLab { get; set; }
    public TimePreference TimePreference { get; set; }  // NEW

    public Subject(string id, string code, string name, SubjectType type,
                  int units, int daysPerWeek, double hoursPerDay,
                  DayPattern preferredPattern = DayPattern.MW, TimePreference timePreference = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        Units = units;
        DaysPerWeek = daysPerWeek;
        HoursPerDay = hoursPerDay;
        ProfessorIds = new List<string>();
        PreferredDayPattern = preferredPattern;
        RequiresLab = false;
        TimePreference = timePreference ?? new TimePreference();  // Default: Mon-Fri 7am-6pm
    }

    public override string ToString() => $"{Code} - {Name}";
}