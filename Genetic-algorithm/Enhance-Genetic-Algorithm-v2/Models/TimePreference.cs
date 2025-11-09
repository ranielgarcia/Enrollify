namespace Enhance_Genetic_Algorithm_v2.Models;
public class TimePreference
{
    public string StartTime { get; set; }  // e.g., "07:00"
    public string EndTime { get; set; }    // e.g., "18:00"
    public List<string> AllowedDays { get; set; }  // e.g., ["Monday", "Tuesday", ...]

    public TimePreference()
    {
        StartTime = "07:00";
        EndTime = "18:00";
        AllowedDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    }

    public bool IsWithinPreference(TimeSlot slot)
    {
        if (!AllowedDays.Contains(slot.Day))
            return false;

        var slotStart = TimeSpan.Parse(slot.StartTime);
        var prefStart = TimeSpan.Parse(StartTime);
        var prefEnd = TimeSpan.Parse(EndTime);

        return slotStart >= prefStart && slotStart < prefEnd;
    }
}