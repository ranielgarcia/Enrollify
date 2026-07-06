using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class NotificationCategoryEnum : SmartEnum<NotificationCategoryEnum>
{
  public static readonly NotificationCategoryEnum System = new NotificationCategoryEnum(nameof(System), 1);
  public static readonly NotificationCategoryEnum Academic = new NotificationCategoryEnum(nameof(Academic), 2);
  public static readonly NotificationCategoryEnum Enrollment = new NotificationCategoryEnum(nameof(Enrollment), 3);
  public static readonly NotificationCategoryEnum Admin = new NotificationCategoryEnum(nameof(Admin), 4);
  public static readonly NotificationCategoryEnum Audit = new NotificationCategoryEnum(nameof(Audit), 5);

  public NotificationCategoryEnum(string name, int value) : base(name, value)
  {

  }
}
