using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Notifications.Specifications;

public class GetUnreadNotificationsForTheUserSpec : Specification<Notification>
{
  public GetUnreadNotificationsForTheUserSpec(UserId userId)
  {
    Query
      .Include(n => n.Recipients)
      .Where(n => (n.TargetUserId != null && n.TargetUserId == userId) ||
                  n.Recipients.Any(r => r.UserId == userId && !r.IsDismissed && !r.IsRead))
      .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTimeOffset.UtcNow);
  }
}
