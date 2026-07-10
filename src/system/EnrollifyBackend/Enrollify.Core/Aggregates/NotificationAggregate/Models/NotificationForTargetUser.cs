using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.NotificationAggregate.Models;

public record NotificationForTargetUser(
  string Type,
  string Title,
  string Message,
  NotificationSeverityEnum Severity,
  NotificationCategoryEnum Category,
  UserId TargetUserId,
  int RetentionDays = NotificationSettings.DefaultRetentionDays,
  NotificationReferenceTypeEnum? ReferenceType = null,
  int? ReferenceId = null);
