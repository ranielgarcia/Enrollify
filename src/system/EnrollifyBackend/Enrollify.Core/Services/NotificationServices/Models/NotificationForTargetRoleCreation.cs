using Enrollify.Core.Constants;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.Core.Services.NotificationServices.Models;

public record NotificationForTargetRoleCreation(
  string Type,
  string Title,
  string Message,
  NotificationCategoryEnum Category,
  RolesEnum[] TargetRoles,
  int RetentionDays = NotificationSettings.DefaultRetentionDays,
  NotificationReferenceTypeEnum? ReferenceType = null,
  int? ReferenceId = null);
