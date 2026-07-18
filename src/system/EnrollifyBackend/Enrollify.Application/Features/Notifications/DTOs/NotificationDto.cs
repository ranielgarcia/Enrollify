using Enrollify.Core.Aggregates.NotificationAggregate;

namespace Enrollify.Application.Features.Notifications.DTOs;

public class NotificationDto
{
  public string Type { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public NotificationSeverityEnum Severity { get; set; }
  public NotificationCategoryEnum Category { get; set; }
  public NotificationReferenceTypeEnum? ReferenceType { get; set; }
  public int? ReferenceId { get; set; }

  public static NotificationDto FromEntity(Notification notification)
  {
    return new NotificationDto
    {
      Type = notification.Type,
      Title = notification.Title,
      Message = notification.Message,
      Severity = notification.Severity,
      Category = notification.Category,
      ReferenceType = notification.ReferenceType,
      ReferenceId = notification.ReferenceId
    };
  }
}
