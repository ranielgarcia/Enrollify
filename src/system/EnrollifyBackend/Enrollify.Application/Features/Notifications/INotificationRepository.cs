using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Notifications;

public interface INotificationRepository
{
  Task<Result> BulkCreate (Notification[] notifications, CancellationToken cancellationToken);
  Task<Result<NotificationId>> Update (Notification notification, CancellationToken cancellationToken);
  Task<Result> Delete (NotificationId id, CancellationToken cancellationToken);
  Task<Result> MarkAllNotificationsAsReadForUser (UserId userId, CancellationToken cancellationToken);
  Task<Result> MarkAllNotificationsAsDismissedForUser (UserId userId, CancellationToken cancellationToken);
}
