using Enrollify.Core.Aggregates.NotificationAggregate.Models;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.NotificationAggregate;

public class Notification : EntityBase<NotificationId>, IAggregateRoot
{
    private Notification()
    {}

    public string Type { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public NotificationSeverityEnum Severity { get; private set; }
    public NotificationCategoryEnum Category { get; private set; }
    public NotificationReferenceTypeEnum? ReferenceType { get; private set; }
    public int? ReferenceId { get; private set; }
    public NotificationTargetScopeEnum TargetScope{ get; private set; }
    public UserId? TargetUserId { get; private set; }
    public RoleId? TargetRoleId { get; private set; }
    public int RetentionDays { get; private set; } = NotificationSettings.DefaultRetentionDays;
    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public User? CreatedByUser { get; private set; }

    private readonly List<NotificationRecipient> _recipients = [];
    public IReadOnlyCollection<NotificationRecipient> Recipients => _recipients.AsReadOnly();

    public static Notification Create(
        BroadcastNotification notification)
    {
        var notif = new Notification
        {
            Type          = notification.Type,
            Title         = notification.Title,
            Message       = notification.Message,
            Severity      = notification.Severity,
            Category      = notification.Category,
            TargetScope   = NotificationTargetScopeEnum.Broadcast,
            RetentionDays = notification.RetentionDays,
            ReferenceType = notification.ReferenceType,
            ReferenceId   = notification.ReferenceId,
            ExpiresAt     = DateTime.UtcNow.AddDays(notification.ReferenceType),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        return notif;
    }

    public static Notification Create(
      NotificationForTargetUser notification)
    {
      var notif = new Notification
      {
        Type          = notification.Type,
        Title         = notification.Title,
        Message       = notification.Message,
        Severity      = notification.Severity,
        Category      = notification.Category,
        TargetScope   = NotificationTargetScopeEnum.User,
        TargetUserId = notification.TargetUserId,
        RetentionDays = notification.RetentionDays,
        ReferenceType = notification.ReferenceType,
        ReferenceId   = notification.ReferenceId,
        ExpiresAt     = DateTime.UtcNow.AddDays(notification.ReferenceType),
        CreatedAt = DateTimeOffset.UtcNow,
      };
      return notif;
    }

    public static Notification Create(
      NotificationForTargetRole notification)
    {
      var notif = new Notification
      {
        Type          = notification.Type,
        Title         = notification.Title,
        Message       = notification.Message,
        Severity      = notification.Severity,
        Category      = notification.Category,
        TargetScope   = NotificationTargetScopeEnum.Role,
        TargetRoleId = notification.TargetRoleId,
        RetentionDays = notification.RetentionDays,
        ReferenceType = notification.ReferenceType,
        ReferenceId   = notification.ReferenceId,
        ExpiresAt     = DateTime.UtcNow.AddDays(notification.ReferenceType),
        CreatedAt = DateTimeOffset.UtcNow,
      };
      return notif;
    }

    public Notification AddCreatedBy(UserId userId)
    {
      CreatedBy = userId;
      return this;
    }

    public Notification AddRecipient(UserId userId)
    {
      if (_recipients.Any(r => (int)r.UserId == (int)userId))
        return this;

      _recipients.Add(new NotificationRecipient(Id, userId));
        return this;
    }
}

