using Enrollify.Core.Aggregates.NotificationAggregate;

namespace Enrollify.Application.Features.Notifications;

public interface INotificationRepository
{
  Task<Result<NotificationId>> Create (Notification notification, CancellationToken cancellationToken);
  Task<Result<NotificationId>> Update (Notification notification, CancellationToken cancellationToken);
  Task<Result> Delete (NotificationId id, CancellationToken cancellationToken);
}
