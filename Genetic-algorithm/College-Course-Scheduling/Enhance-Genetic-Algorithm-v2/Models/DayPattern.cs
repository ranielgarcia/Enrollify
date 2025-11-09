namespace Enhance_Genetic_Algorithm_v2.Models;

public enum DayPattern
{
    // Two-day patterns
    MW,          // Monday-Wednesday
    TTh,         // Tuesday-Thursday
    WF,          // Wednesday-Friday
    MTh,         // Monday-Thursday
    TF,          // Tuesday-Friday
    
    // Three-day patterns
    MWF,         // Monday-Wednesday-Friday
    MWTh,        // Monday-Wednesday-Thursday
    MTuTh,       // Monday-Tuesday-Thursday
    TuWTh,       // Tuesday-Wednesday-Thursday
    TuWF,        // Tuesday-Wednesday-Friday
    WThF,        // Wednesday-Thursday-Friday
    
    // Four-day patterns
    MTuWTh,      // Monday-Tuesday-Wednesday-Thursday
    MTuWF,       // Monday-Tuesday-Wednesday-Friday
    MTuThF,      // Monday-Tuesday-Thursday-Friday
    MWThF,       // Monday-Wednesday-Thursday-Friday
    TuWThF,      // Tuesday-Wednesday-Thursday-Friday
    
    // Five-day pattern
    Daily,       // Monday to Friday (MTWTHF)
    
    // Single day patterns
    Single,      // Any single day
    Monday,      // Monday only
    Tuesday,     // Tuesday only
    Wednesday,   // Wednesday only
    Thursday,    // Thursday only
    Friday,      // Friday only
    
    // Weekend patterns (for special programs/continuing education)
    Saturday,    // Saturday only
    Sunday,      // Sunday only
    Weekend,     // Saturday-Sunday
    
    // Special patterns
    Flexible     // No fixed pattern (arranged/online/hybrid)
}

public static class DayPatternExtensions
{
    public static int GetDayCount(this DayPattern pattern)
    {
        return pattern switch
        {
            // Two-day patterns
            DayPattern.MW => 2,
            DayPattern.TTh => 2,
            DayPattern.WF => 2,
            DayPattern.MTh => 2,
            DayPattern.TF => 2,
            
            // Three-day patterns
            DayPattern.MWF => 3,
            DayPattern.MWTh => 3,
            DayPattern.MTuTh => 3,
            DayPattern.TuWTh => 3,
            DayPattern.TuWF => 3,
            DayPattern.WThF => 3,
            
            // Four-day patterns
            DayPattern.MTuWTh => 4,
            DayPattern.MTuWF => 4,
            DayPattern.MTuThF => 4,
            DayPattern.MWThF => 4,
            DayPattern.TuWThF => 4,
            
            // Five-day pattern
            DayPattern.Daily => 5,
            
            // Single day patterns
            DayPattern.Single => 1,
            DayPattern.Monday => 1,
            DayPattern.Tuesday => 1,
            DayPattern.Wednesday => 1,
            DayPattern.Thursday => 1,
            DayPattern.Friday => 1,
            DayPattern.Saturday => 1,
            DayPattern.Sunday => 1,
            
            // Weekend patterns
            DayPattern.Weekend => 2,
            
            // Special patterns
            DayPattern.Flexible => 0,
            
            _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
        };
    }
    
    public static DayOfWeek[] GetDaysOfWeek(this DayPattern pattern)
    {
        return pattern switch
        {
            // Two-day patterns
            DayPattern.MW => new[] { DayOfWeek.Monday, DayOfWeek.Wednesday },
            DayPattern.TTh => new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday },
            DayPattern.WF => new[] { DayOfWeek.Wednesday, DayOfWeek.Friday },
            DayPattern.MTh => new[] { DayOfWeek.Monday, DayOfWeek.Thursday },
            DayPattern.TF => new[] { DayOfWeek.Tuesday, DayOfWeek.Friday },
            
            // Three-day patterns
            DayPattern.MWF => new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
            DayPattern.MWTh => new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
            DayPattern.MTuTh => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday },
            DayPattern.TuWTh => new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
            DayPattern.TuWF => new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday },
            DayPattern.WThF => new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
            
            // Four-day patterns
            DayPattern.MTuWTh => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
            DayPattern.MTuWF => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday },
            DayPattern.MTuThF => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday },
            DayPattern.MWThF => new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
            DayPattern.TuWThF => new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
            
            // Five-day pattern
            DayPattern.Daily => new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
            
            // Single day patterns
            DayPattern.Monday => new[] { DayOfWeek.Monday },
            DayPattern.Tuesday => new[] { DayOfWeek.Tuesday },
            DayPattern.Wednesday => new[] { DayOfWeek.Wednesday },
            DayPattern.Thursday => new[] { DayOfWeek.Thursday },
            DayPattern.Friday => new[] { DayOfWeek.Friday },
            DayPattern.Saturday => new[] { DayOfWeek.Saturday },
            DayPattern.Sunday => new[] { DayOfWeek.Sunday },
            
            // Weekend patterns
            DayPattern.Weekend => new[] { DayOfWeek.Saturday, DayOfWeek.Sunday },
            
            // Special patterns
            DayPattern.Single => Array.Empty<DayOfWeek>(),
            DayPattern.Flexible => Array.Empty<DayOfWeek>(),
            
            _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
        };
    }
    
    public static string GetDescription(this DayPattern pattern)
    {
        return pattern switch
        {
            DayPattern.MW => "Monday, Wednesday",
            DayPattern.TTh => "Tuesday, Thursday",
            DayPattern.WF => "Wednesday, Friday",
            DayPattern.MTh => "Monday, Thursday",
            DayPattern.TF => "Tuesday, Friday",
            DayPattern.MWF => "Monday, Wednesday, Friday",
            DayPattern.MWTh => "Monday, Wednesday, Thursday",
            DayPattern.MTuTh => "Monday, Tuesday, Thursday",
            DayPattern.TuWTh => "Tuesday, Wednesday, Thursday",
            DayPattern.TuWF => "Tuesday, Wednesday, Friday",
            DayPattern.WThF => "Wednesday, Thursday, Friday",
            DayPattern.MTuWTh => "Monday, Tuesday, Wednesday, Thursday",
            DayPattern.MTuWF => "Monday, Tuesday, Wednesday, Friday",
            DayPattern.MTuThF => "Monday, Tuesday, Thursday, Friday",
            DayPattern.MWThF => "Monday, Wednesday, Thursday, Friday",
            DayPattern.TuWThF => "Tuesday, Wednesday, Thursday, Friday",
            DayPattern.Daily => "Monday through Friday (Daily)",
            DayPattern.Single => "Any Single Day",
            DayPattern.Monday => "Monday Only",
            DayPattern.Tuesday => "Tuesday Only",
            DayPattern.Wednesday => "Wednesday Only",
            DayPattern.Thursday => "Thursday Only",
            DayPattern.Friday => "Friday Only",
            DayPattern.Saturday => "Saturday Only",
            DayPattern.Sunday => "Sunday Only",
            DayPattern.Weekend => "Saturday, Sunday",
            DayPattern.Flexible => "Flexible/Arranged Schedule",
            _ => pattern.ToString()
        };
    }
    
    public static bool IncludesDay(this DayPattern pattern, DayOfWeek day)
    {
        var days = pattern.GetDaysOfWeek();
        return days.Contains(day);
    }
}