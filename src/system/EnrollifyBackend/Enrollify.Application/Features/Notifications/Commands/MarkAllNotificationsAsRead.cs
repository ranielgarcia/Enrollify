using Enrollify.Core.Authentication;

namespace Enrollify.Application.Features.Notifications.Commands;

public static class MarkAllNotificationsAsRead
{
  public sealed record Command() : IRequest<Unit>;

  public sealed class Handler : IRequestHandler<Command, Unit>
  {
      private readonly INotificationRepository _notificationRepository;
      private readonly ICurrentUserAccessor _currentUserAccessor;

      public Handler(
        INotificationRepository notificationRepository,
        ICurrentUserAccessor currentUserAccessor)
      {
        _notificationRepository = notificationRepository;
        _currentUserAccessor = currentUserAccessor;
      }

      public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
      {
        var user = _currentUserAccessor.GetCurrentUser();
        await _notificationRepository.MarkAllNotificationsAsReadForUser(user!.Id, cancellationToken);
        return Unit.Value;
      }
  }
}
