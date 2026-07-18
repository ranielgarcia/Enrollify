using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Notifications.Specifications;

public class GetNotificationByUserAndIDSpec : Specification<Notification>
{
  public GetNotificationByUserAndIDSpec(UserId userId, NotificationId notificationId)
  {
    Query
      .Include(n => n.Recipients.Where(r => r.UserId == userId))
      .Where(n => (n.TargetUserId != null && n.TargetUserId == userId) ||
                  n.Recipients.Any(r => r.UserId == userId))
      .Where(n => n.Id == notificationId);
  }
}
