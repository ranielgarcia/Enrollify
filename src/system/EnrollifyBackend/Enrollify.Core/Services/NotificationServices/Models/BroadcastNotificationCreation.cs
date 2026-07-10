using Enrollify.Core.Constants;

namespace Enrollify.Core.Services.NotificationServices.Models;

public record BroadcastNotificationCreation(
  string Type,
  string Title,
  string Message,
  NotificationCategoryEnum Category,
  int RetentionDays = NotificationSettings.DefaultRetentionDays,
  NotificationReferenceTypeEnum? ReferenceType = null,
  int? ReferenceId = null);
