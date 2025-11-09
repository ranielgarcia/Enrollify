namespace Enhance_Genetic_Algorithm_v2.Models;
public class DayAndTimePreference
{
    public int DaysPerWeek { get; private set; } // How many days per week (1-5)
    public DayPattern PreferredDayPattern { get; set; }
    public string TimeWindowStart { get; set; }
    public string TimeWindowEnd { get; set; }
    public List<string> AllowedDays { get; set; } 

    public DayAndTimePreference(DayPattern preferredDayPattern)
    {
        PreferredDayPattern = preferredDayPattern;
        DaysPerWeek = PreferredDayPattern.GetDayCount();

        // Regular
        TimeWindowStart = "07:00";
        TimeWindowEnd = "18:00";
        AllowedDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    }

    public bool IsWithinPreference(TimeSlot slot)
    {
        if (!AllowedDays.Contains(slot.Day))
            return false;

        var slotStart = TimeSpan.Parse(slot.StartTime);
        var prefStart = TimeSpan.Parse(TimeWindowStart);
        var prefEnd = TimeSpan.Parse(TimeWindowEnd);

        return slotStart >= prefStart && slotStart < prefEnd;
    }
}
