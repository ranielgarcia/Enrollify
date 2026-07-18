using Enrollify.Application.Features.Notifications.Specifications;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Authentication;

namespace Enrollify.Application.Features.Notifications.Queries;

public record GetUnreadNotificationsCountForTheUserQuery() : IRequest<int>;

public class GetUnreadNotificationsCountForTheUserQueryHandler : IRequestHandler<GetUnreadNotificationsCountForTheUserQuery, int>
{
  private readonly IReadRepository<Notification> _readRepository;
  private readonly ICurrentUserAccessor _currentUserAccessor;

  public GetUnreadNotificationsCountForTheUserQueryHandler(IReadRepository<Notification> readRepository, ICurrentUserAccessor currentUserAccessor)
  {
    _readRepository = readRepository;
    _currentUserAccessor = currentUserAccessor;
  }

  public async Task<int> Handle(GetUnreadNotificationsCountForTheUserQuery request, CancellationToken cancellationToken)
  {
    var user = _currentUserAccessor.GetCurrentUser();
    if (user is null) return 0;

    var spec = new GetUnreadNotificationsForTheUserSpec(user.Id);

    var totalCount = await _readRepository.CountAsync(spec, cancellationToken);
    return totalCount;
  }
}
