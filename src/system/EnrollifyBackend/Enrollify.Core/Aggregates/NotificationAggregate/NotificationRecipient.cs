using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.NotificationAggregate;

public class NotificationRecipient : EntityBase<NotificationRecipientId>
{
  private NotificationRecipient()
  {}

  public NotificationRecipient(NotificationId notificationId, UserId userId)
  {
    NotificationId = notificationId;
    UserId = userId;
  }
  public NotificationId NotificationId { get; set; }
  public UserId UserId { get; set; }
  public bool IsRead { get; set; }
  public DateTimeOffset? ReadAt { get; set; }
  public bool IsDismissed { get; set; }
  public DateTimeOffset? DismissedAt { get; set; }
}
