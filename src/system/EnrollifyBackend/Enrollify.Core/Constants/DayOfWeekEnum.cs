using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class DayOfWeekEnum : SmartEnum<DayOfWeekEnum, string>
{
  public int Order { get; set; }

  public static readonly DayOfWeekEnum Sunday = new DayOfWeekEnum("Sunday", "SUN", 0);
  public static readonly DayOfWeekEnum Monday = new DayOfWeekEnum("Monday", "MON", 1);
  public static readonly DayOfWeekEnum Tuesday = new DayOfWeekEnum("Tuesday", "TUE", 2);
  public static readonly DayOfWeekEnum Wednesday = new DayOfWeekEnum("Wednesday", "WED", 3);
  public static readonly DayOfWeekEnum Thursday = new DayOfWeekEnum("Thursday", "THU", 4);
  public static readonly DayOfWeekEnum Friday = new DayOfWeekEnum("Friday", "FRI", 5);
  public static readonly DayOfWeekEnum Saturday = new DayOfWeekEnum("Saturday", "SAT", 6);
  protected DayOfWeekEnum(string name, string value, int order) : base(name, value)
  {
    Order = order;
  }
}
