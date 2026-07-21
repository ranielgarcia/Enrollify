using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Notifications.Specifications;

public class FilterNotificationsForTheUserPaginatedSpec : Specification<Notification>
{
  public FilterNotificationsForTheUserPaginatedSpec(
    UserId userId, string searchTerm = "", int page = 1, int pageSize = 10, NotificationCategoryEnum? category = null, NotificationSeverityEnum? severity = null, string? readState = null)
  {
    Query
      .Include(n => n.Recipients.Where(r => r.UserId == userId))
      .AsNoTracking()
      .Include(n => n.CreatedByUser);

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
      Query.Search(n => n.Title, searchTerm);
      Query.Search(n => n.Message, searchTerm);
    }

    if (category != null)
    {
      Query.Where(n => n.Category == category);
    }

    if (severity != null)
    {
      Query.Where(n => n.Severity == severity);
    }

    if (!string.IsNullOrEmpty(readState))
    {
      if (readState.ToLower() == "read")
        Query.Where(n => n.Recipients.Any(r => r.IsRead && r.UserId == userId));
      else if  (readState.ToLower() == "unread")
        Query.Where(n => n.Recipients.Any(r => !r.IsRead && r.UserId == userId));
    }

    Query
      .Where(n => (n.TargetUserId != null && n.TargetUserId == userId) || n.Recipients.Any(r => r.UserId == userId && !r.IsDismissed))
      .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTimeOffset.UtcNow)
      .OrderByDescending(n => n.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize);
  }
}
