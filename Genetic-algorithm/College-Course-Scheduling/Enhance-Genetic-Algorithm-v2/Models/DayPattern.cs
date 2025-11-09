namespace Enhance_Genetic_Algorithm_v2.Models;
public enum DayPattern
{
    MW,          // Monday-Wednesday
    TTh,         // Tuesday-Thursday
    MWF,         // Monday-Wednesday-Friday
    Daily,       // Monday to Friday
    Single       // Any single day
}

public static class DayPatternExtensions
{
    public const int MWDays = 2;
    public const int TThDays = 2;
    public const int MWFDays = 3;
    public const int DailyDays = 5;
    public const int SingleDays = 1;

    public static int GetDayCount(this DayPattern pattern)
    {
        return pattern switch
        {
            DayPattern.MW => MWDays,
            DayPattern.TTh => TThDays,
            DayPattern.MWF => MWFDays,
            DayPattern.Daily => DailyDays,
            DayPattern.Single => SingleDays,
            _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
        };
    }
}