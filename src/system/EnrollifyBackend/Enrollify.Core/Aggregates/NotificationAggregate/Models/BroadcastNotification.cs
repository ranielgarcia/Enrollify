using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.NotificationAggregate.Models;

public record BroadcastNotification(
  string Type,
  string Title,
  string Message,
  NotificationSeverityEnum Severity,
  NotificationCategoryEnum Category,
  int RetentionDays = NotificationSettings.DefaultRetentionDays,
  NotificationReferenceTypeEnum? ReferenceType = null,
  int? ReferenceId = null);
