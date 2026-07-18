using Enrollify.Application.Features.Notifications.Specifications;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Authentication;

namespace Enrollify.Application.Features.Notifications.Commands;

public static class MarkNotificationAsReadOrUnreadOrDismissed
{
  public enum Action
  {
    MarkRead,
    MarkUnread,
    MarkDismissed
  }
  public sealed record Command (NotificationId NotificationId, Action Action) : IRequest<Unit>;

  public sealed class Handler : IRequestHandler<Command, Unit>
  {
    private readonly IReadRepository<Notification> _readRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public Handler(
      IReadRepository<Notification> readRepository,
      INotificationRepository notificationRepository,
      ICurrentUserAccessor currentUserAccessor)
    {
      _readRepository = readRepository;
      _notificationRepository = notificationRepository;
      _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
    {
      var user = _currentUserAccessor.GetCurrentUser();
      var spec = new GetNotificationByUserAndIDSpec(user!.Id, command.NotificationId);
      var notification = await _readRepository.FirstOrDefaultAsync(spec, cancellationToken);
      if (notification == null)
      {
        throw new Exception($"Notification with id {command.NotificationId} not found");
      }

      if (command.Action == Action.MarkRead)
        notification.Recipients.FirstOrDefault(x => x.UserId == user!.Id)?.MarkAsRead();

      if (command.Action == Action.MarkUnread)
        notification.Recipients.FirstOrDefault(x => x.UserId == user!.Id)?.MarkAsUnread();

      if (command.Action == Action.MarkDismissed)
        notification.Recipients.FirstOrDefault(x => x.UserId == user!.Id)?.MarkAsDismissed();

      await _notificationRepository.Update(notification, cancellationToken);
      return Unit.Value;
    }
  }
}
