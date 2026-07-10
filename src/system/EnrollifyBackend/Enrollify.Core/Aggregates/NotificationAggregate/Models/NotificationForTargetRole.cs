using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Aggregates.NotificationAggregate.Models;

public record NotificationForTargetRole(
  string Type,
  string Title,
  string Message,
  NotificationSeverityEnum Severity,
  NotificationCategoryEnum Category,
  RoleId TargetRoleId,
  int RetentionDays = NotificationSettings.DefaultRetentionDays,
  NotificationReferenceTypeEnum? ReferenceType = null,
  int? ReferenceId = null);
