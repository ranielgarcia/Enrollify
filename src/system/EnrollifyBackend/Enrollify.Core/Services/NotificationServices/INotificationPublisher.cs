using Enrollify.Core.Services.NotificationServices.Models;

namespace Enrollify.Core.Services.NotificationServices;

public interface INotificationPublisher
{
  // Broadcast notifications
  Task InfoBroadcastNotification(BroadcastNotificationCreation notification);
  Task WarningBroadcastNotification(BroadcastNotificationCreation notification);
  Task ErrorBroadcastNotification(BroadcastNotificationCreation notification);
  Task SuccessBroadcastNotification(BroadcastNotificationCreation notification);

  // Target User notifications
  Task InfoTargetUserNotification(NotificationForTargetUserCreation notification);
  Task WarningTargetUserNotification(NotificationForTargetUserCreation notification);
  Task ErrorTargetUserNotification(NotificationForTargetUserCreation notification);
  Task SuccessTargetUserNotification(NotificationForTargetUserCreation notification);

  // Target Role notifications
  Task InfoTargetRoleNotification(NotificationForTargetRoleCreation notification);
  Task WarningTargetRoleNotification(NotificationForTargetRoleCreation notification);
  Task ErrorTargetRoleNotification(NotificationForTargetRoleCreation notification);
  Task SuccessTargetRoleNotification(NotificationForTargetRoleCreation notification);
}
