using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class NotificationSeverityEnum : SmartEnum<NotificationSeverityEnum>
{
  public static readonly NotificationSeverityEnum Info = new NotificationSeverityEnum(nameof(Info), 1);
  public static readonly NotificationSeverityEnum Warning = new NotificationSeverityEnum(nameof(Warning), 2);
  public static readonly NotificationSeverityEnum Error = new NotificationSeverityEnum(nameof(Error), 3);
  public static readonly NotificationSeverityEnum Success = new NotificationSeverityEnum(nameof(Success), 4);

  public NotificationSeverityEnum(string name, int value) : base(name, value)
  {

  }
}
