using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class NotificationReferenceTypeEnum : SmartEnum<NotificationReferenceTypeEnum>
{
  public static readonly NotificationReferenceTypeEnum Curriculum = new (nameof(Curriculum), 1);
  public static readonly NotificationReferenceTypeEnum Scheduling = new (nameof(Scheduling), 2);

  public NotificationReferenceTypeEnum (string name, int value) : base(name, value)
  {
  }
}
