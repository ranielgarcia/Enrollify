namespace Enhance_Genetic_Algorithm.Models;
public class TimeSlot
{
    public string Id { get; set; }
    public string Day { get; set; }
    public string Time { get; set; }

    public TimeSlot(string id, string day, string time)
    {
        Id = id;
        Day = day;
        Time = time;
    }

    public override string ToString() => $"{Day} {Time}";
}