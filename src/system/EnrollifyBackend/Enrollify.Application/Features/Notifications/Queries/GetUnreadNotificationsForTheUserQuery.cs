using Enrollify.Application.Features.Notifications.Specifications;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Authentication;

namespace Enrollify.Application.Features.Notifications.Queries;

public record GetUnreadNotificationsForTheUserQuery() : IRequest<int>;

public class GetUnreadNotificationsForTheUserQueryHandler : IRequestHandler<GetUnreadNotificationsForTheUserQuery, int>
{
  private readonly IReadRepository<Notification> _readRepository;
  private readonly ICurrentUserAccessor _currentUserAccessor;

  public GetUnreadNotificationsForTheUserQueryHandler(IReadRepository<Notification> readRepository, ICurrentUserAccessor currentUserAccessor)
  {
    _readRepository = readRepository;
    _currentUserAccessor = currentUserAccessor;
  }

  public async Task<int> Handle(GetUnreadNotificationsForTheUserQuery request, CancellationToken cancellationToken)
  {
    var user = _currentUserAccessor.GetCurrentUser();
    if (user is null) return 0;

    var spec = new GetUnreadNotificationsForTheUserSpec(user.Id);

    var totalCount = await _readRepository.CountAsync(spec, cancellationToken);
    return totalCount;
  }
}
