using System.Linq.Expressions;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.Notifications.Specifications;

public class FilterNotificationsForTheUserPaginatedSpec : Specification<Notification>
{
  private static readonly HashSet<string> AllowedFilterColumns =
  [
    "searchterm",
    "readstate",
    nameof(Notification.Category).ToLower(),
    nameof(Notification.Severity).ToLower()
  ];

  public FilterNotificationsForTheUserPaginatedSpec(UserId userId, string searchTerm = "", int page = 1, int pageSize = 10, IEnumerable<FilterItem>? filters = null)
  {
    Query
      .AsNoTracking()
      .Include(n => n.CreatedByUser);

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
      Query.Search(n => n.Title, searchTerm);
    }

    var filterExpressions = new List<Expression<Func<Notification, bool>>>();

    foreach (var filter in filters ?? [])
    {
      var filterId = filter.Id.ToLower();
      if (!AllowedFilterColumns.Contains(filterId, StringComparer.InvariantCultureIgnoreCase)) continue;

      if (filterId == "readstate" && !string.IsNullOrEmpty(filter.Value))
      {
        if (filter.Value?.ToLower() == "read")
          filterExpressions.Add(n => n.Recipients.Any(r => r.IsRead));
        else if  (filter.Value?.ToLower() == "unread")
          filterExpressions.Add(n => n.Recipients.Any(r => !r.IsRead));
      }

      var expr = filterId switch
      {
        "searchterm" => FilterExpressionBuilder.ForString<Notification>(n => n.Title + " " + n.Message, filter),
        "category" => FilterExpressionBuilder.ForString<Notification>(n => (string)n.Category.Name, filter),
        "severity" => FilterExpressionBuilder.ForString<Notification>(n => (string)n.Severity.Name, filter),
        _ => null
      };

      if (expr is not null) filterExpressions.Add(expr);
    }

    if (filterExpressions.Count > 0)
    {
      Expression<Func<Notification, bool>>? combined = FilterExpressionBuilder.Combine(filterExpressions, JoinOperator.And);
      if (combined is not null) Query.Where(combined);
    }

    Query
      .Where(n => (n.TargetUserId != null && n.TargetUserId == userId) || n.Recipients.Any(r => r.UserId == userId && !r.IsDismissed))
      .Where(n => n.ExpiresAt == null)
      .OrderByDescending(n => n.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize);
  }
}
