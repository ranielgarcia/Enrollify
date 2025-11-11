namespace Enhance_Genetic_Algorithm_v2.Models;
public class TimeSlot
{
    public string Id { get; set; }
    public string Day { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }

    public TimeSlot(string id, string day, string startTime, string endTime)
    {
        Id = id;
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
    }

    // Calculate duration in hours
    public double GetDurationHours()
    {
        var start = TimeSpan.Parse(StartTime);
        var end = TimeSpan.Parse(EndTime);
        return (end - start).TotalHours;
    }

    // NEW: Check if two timeslots overlap
    public bool OverlapsWith(TimeSlot other)
    {
        if (Day != other.Day)
            return false;

        var thisStart = TimeSpan.Parse(StartTime);
        var thisEnd = TimeSpan.Parse(EndTime);
        var otherStart = TimeSpan.Parse(other.StartTime);
        var otherEnd = TimeSpan.Parse(other.EndTime);

        // Check for overlap: start1 < end2 AND start2 < end1
        return thisStart < otherEnd && otherStart < thisEnd;
    }

    public string GetTimePattern()
    {
        return $"{StartTime}-{EndTime}";
    }

    public override string ToString() => $"{Day} {StartTime}-{EndTime}";
}