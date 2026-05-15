using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class DayOfWeekEnum : SmartEnum<DayOfWeekEnum, string>
{
    public static readonly DayOfWeekEnum Sunday = new DayOfWeekEnum("Sunday", "SUN");
    public static readonly DayOfWeekEnum Monday = new DayOfWeekEnum("Monday", "MON");
    public static readonly DayOfWeekEnum Tuesday = new DayOfWeekEnum("Tuesday", "TUE");
    public static readonly DayOfWeekEnum Wednesday = new DayOfWeekEnum("Wednesday", "WED");
    public static readonly DayOfWeekEnum Thursday = new DayOfWeekEnum("Thursday", "THU");
    public static readonly DayOfWeekEnum Friday = new DayOfWeekEnum("Friday", "FRI");
    public static readonly DayOfWeekEnum Saturday = new DayOfWeekEnum("Saturday", "SAT");
    protected DayOfWeekEnum(string name, string value) : base(name, value) { }
}
