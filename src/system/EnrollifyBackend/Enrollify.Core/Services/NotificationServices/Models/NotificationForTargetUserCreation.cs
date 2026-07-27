using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Core.Services.NotificationServices.Models;

public record NotificationForTargetUserCreation(
  string Type,
  string Title,
  string Message,
  NotificationCategoryEnum Category,
  UserId? TargetUserId = null,
  int RetentionDays = NotificationSettings.DefaultRetentionDays,
  NotificationReferenceTypeEnum? ReferenceType = null,
  int? ReferenceId = null);
