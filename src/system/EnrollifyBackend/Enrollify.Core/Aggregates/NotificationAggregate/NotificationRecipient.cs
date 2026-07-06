using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.NotificationAggregate;

public class NotificationRecipient : EntityBase<NotificationRecipientId>, IAuditable
{
  private NotificationRecipient()
  {}

  public NotificationRecipient(NotificationId notificationId, UserId userId)
  {
    NotificationId = notificationId;
    UserId = userId;
    IsActive = true;
  }
  public NotificationId NotificationId { get; set; }
  public UserId UserId { get; set; }
  public bool IsRead { get; set; }
  public DateTimeOffset? ReadAt { get; set; }
  public bool IsDismissed { get; set; }
  public DateTimeOffset? DismissedAt { get; set; }

  public DateTimeOffset CreatedAt { get; private set; }
  public UserId CreatedBy { get; private set; }
  public User? CreatedByUser { get; private set; }
  public DateTimeOffset? UpdatedAt { get; private set; }
  public UserId? UpdatedBy { get; private set; }
  public User? UpdatedByUser { get; private set; }
  public DateTimeOffset? DeletedAt { get; private set; }
  public UserId? DeletedBy { get; private set; }
  public User? DeletedByUser { get; private set; }
  public bool IsActive { get; private set; }
}
