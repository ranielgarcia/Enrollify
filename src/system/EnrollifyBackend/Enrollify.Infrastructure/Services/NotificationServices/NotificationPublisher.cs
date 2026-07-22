using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.NotificationAggregate.Models;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Authentication;
using Enrollify.Core.Constants;
using Enrollify.Core.Services.NotificationServices;
using Enrollify.Core.Services.NotificationServices.Models;

namespace Enrollify.Infrastructure.Services.NotificationServices;

public class NotificationPublisher : INotificationPublisher
{
  private readonly ICurrentUserAccessor _currentUserAccessor;
  private readonly INotificationBus _notificationBus;

  public NotificationPublisher(
    ICurrentUserAccessor currentUserAccessor,
    INotificationBus notificationBus)
  {
    _currentUserAccessor = currentUserAccessor;
    _notificationBus = notificationBus;
  }

  // Broadcast notifications
  public async Task InfoBroadcastNotification(BroadcastNotificationCreation notification)
  {
    await BroadcastNotification(notification, NotificationSeverityEnum.Info);
  }

  public async Task WarningBroadcastNotification(BroadcastNotificationCreation notification)
  {
    await BroadcastNotification(notification, NotificationSeverityEnum.Warning);
  }

  public async Task ErrorBroadcastNotification(BroadcastNotificationCreation notification)
  {
    await BroadcastNotification(notification, NotificationSeverityEnum.Error);
  }

  public async Task SuccessBroadcastNotification(BroadcastNotificationCreation notification)
  {
    await BroadcastNotification(notification, NotificationSeverityEnum.Success);
  }

  // Target User notifications

  public async Task InfoTargetUserNotification(NotificationForTargetUserCreation notification)
  {
    await NotifyTargetUser(notification, NotificationSeverityEnum.Info);
  }

  public async Task WarningTargetUserNotification(NotificationForTargetUserCreation notification)
  {
    await NotifyTargetUser(notification, NotificationSeverityEnum.Warning);
  }

  public async Task ErrorTargetUserNotification(NotificationForTargetUserCreation notification)
  {
    await NotifyTargetUser(notification, NotificationSeverityEnum.Error);
  }

  public async Task SuccessTargetUserNotification(NotificationForTargetUserCreation notification)
  {
    await NotifyTargetUser(notification, NotificationSeverityEnum.Success);
  }

  // Target Role notifications
  public async Task InfoTargetRoleNotification(NotificationForTargetRoleCreation notification)
  {
    await NotifyTargetUsersWithARole(notification, NotificationSeverityEnum.Info);
  }

  public async Task WarningTargetRoleNotification(NotificationForTargetRoleCreation notification)
  {
    await NotifyTargetUsersWithARole(notification, NotificationSeverityEnum.Warning);
  }

  public async Task ErrorTargetRoleNotification(NotificationForTargetRoleCreation notification)
  {
    await NotifyTargetUsersWithARole(notification, NotificationSeverityEnum.Error);
  }

  public async Task SuccessTargetRoleNotification(NotificationForTargetRoleCreation notification)
  {
    await NotifyTargetUsersWithARole(notification, NotificationSeverityEnum.Success);
  }

  private async Task BroadcastNotification(BroadcastNotificationCreation notification, NotificationSeverityEnum severity)
  {
    var currentUser = _currentUserAccessor.GetCurrentUser();
    var userId = currentUser?.Id ??  SystemUserConstants.SystemUserId;
    var broadcastNotification = new BroadcastNotification(
      notification.Type,
      notification.Title,
      notification.Message,
      severity,
      notification.Category,
      notification.RetentionDays,
      notification.ReferenceType,
      notification.ReferenceId
    );

    var notificationEntity = Notification.Create(broadcastNotification);
    notificationEntity.AddCreatedBy(userId);

    await _notificationBus.PublishAsync(new NotificationCreatedEvent(notificationEntity));
  }

  private async Task NotifyTargetUser(NotificationForTargetUserCreation notification, NotificationSeverityEnum severity)
  {
    var currentUser = _currentUserAccessor.GetCurrentUser();
    var userId = currentUser?.Id ??  SystemUserConstants.SystemUserId;
    var broadcastNotification = new NotificationForTargetUser(
      notification.Type,
      notification.Title,
      notification.Message,
      severity,
      notification.Category,
      notification.TargetUserId ?? userId,
      notification.RetentionDays,
      notification.ReferenceType,
      notification.ReferenceId
    );

    var notificationEntity = Notification.Create(broadcastNotification);
    notificationEntity.AddCreatedBy(userId);

    await _notificationBus.PublishAsync(new NotificationCreatedEvent(notificationEntity));
  }

  private async Task NotifyTargetUsersWithARole(NotificationForTargetRoleCreation notification, NotificationSeverityEnum severity)
  {
    var currentUser = _currentUserAccessor.GetCurrentUser();
    var userId = currentUser?.Id ??  SystemUserConstants.SystemUserId;

    foreach (var targetRole in notification.TargetRoles)
    {
      var broadcastNotification = new NotificationForTargetRole(
        notification.Type,
        notification.Title,
        notification.Message,
        severity,
        notification.Category,
        RoleId.From(targetRole.Value),
        notification.RetentionDays,
        notification.ReferenceType,
        notification.ReferenceId
      );

      var notificationEntity = Notification.Create(broadcastNotification);
      notificationEntity.AddCreatedBy(userId);

      await _notificationBus.PublishAsync(new NotificationCreatedEvent(notificationEntity));
    }
  }
}
