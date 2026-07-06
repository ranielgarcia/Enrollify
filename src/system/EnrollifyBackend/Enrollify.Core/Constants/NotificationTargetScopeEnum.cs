using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class NotificationTargetScopeEnum : SmartEnum<NotificationTargetScopeEnum>
{
  public static readonly NotificationTargetScopeEnum User = new NotificationTargetScopeEnum(nameof(User), 1);
  public static readonly NotificationTargetScopeEnum Role = new NotificationTargetScopeEnum(nameof(Role), 2);
  public static readonly NotificationTargetScopeEnum Broadcast = new NotificationTargetScopeEnum(nameof(Broadcast), 3);

  public NotificationTargetScopeEnum(string name, int value) : base(name, value)
  {

  }
}
