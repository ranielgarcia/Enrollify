using Enrollify.Core.Aggregates.NotificationAggregate;

namespace Enrollify.Application.Features.Notifications.DTOs;

public class NotificationDto
{
  public int Id { get; set; }
  public string Type { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string Severity { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string? ReferenceType { get; set; }
  public int? ReferenceId { get; set; }
  public DateTimeOffset CreatedAt { get; private set; }
  public bool IsRead { get; set; }

  public static NotificationDto FromEntity(Notification notification)
  {
    return new NotificationDto
    {
      Id = notification.Id.Value,
      Type = notification.Type,
      Title = notification.Title,
      Message = notification.Message,
      Severity = notification.Severity.Name,
      Category = notification.Category.Name,
      ReferenceType = notification.ReferenceType?.Name,
      ReferenceId = notification.ReferenceId,
      CreatedAt = notification.CreatedAt,
      IsRead = notification.Recipients.FirstOrDefault()?.IsRead ?? false,
    };
  }
}
