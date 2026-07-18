using Enrollify.Application.Features.Notifications.DTOs;
using Enrollify.Application.Features.Notifications.Specifications;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Authentication;

namespace Enrollify.Application.Features.Notifications.Queries;

public record FilterNotificationsPaginatedQuery (
  string SearchTerm = "",
  int Page = 1,
  int PageSize = 10,
  IEnumerable<FilterItem>? Filters = null) : IRequest<Result<PagedResult<NotificationDto>>>;

public class FilterNotificationsPaginatedQueryHandler : IRequestHandler<FilterNotificationsPaginatedQuery, Result<PagedResult<NotificationDto>>>
{
  private readonly IReadRepository<Notification> _readRepository;
  private readonly ICurrentUserAccessor _currentUserAccessor;

  public FilterNotificationsPaginatedQueryHandler(IReadRepository<Notification> readRepository, ICurrentUserAccessor currentUserAccessor)
  {
    _readRepository = readRepository;
    _currentUserAccessor = currentUserAccessor;
  }

  public async Task<Result<PagedResult<NotificationDto>>> Handle(FilterNotificationsPaginatedQuery request, CancellationToken cancellationToken)
  {
    var user = _currentUserAccessor.GetCurrentUser();
    var spec = new FilterNotificationsForTheUserPaginatedSpec(user!.Id, request.SearchTerm, request.Page, request.PageSize, request.Filters);
    var notifications = await _readRepository.ListAsync(spec, cancellationToken);
    var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

    var items = notifications.Select(NotificationDto.FromEntity).ToList();

    return new PagedResult<NotificationDto>(
      items?.AsReadOnly() ?? Array.Empty<NotificationDto>().AsReadOnly(),
      request.Page,
      request.PageSize,
      totalCount);
  }
}
