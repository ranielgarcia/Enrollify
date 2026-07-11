using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Services;
using Enrollify.Core.Services.NotificationServices.Models;

namespace Enrollify.Application.Features.Notifications;

public class OnNotificationCreatedEventHandler
{
  private readonly INotificationRepository _notificationRepository;
  private readonly IUserQueryService _userQueryService;

  public OnNotificationCreatedEventHandler(INotificationRepository notificationRepository, IUserQueryService userQueryService)
  {
    _notificationRepository = notificationRepository;
    _userQueryService = userQueryService;
  }

  public async Task Handle(NotificationCreatedEvent[] notificationCreatedEvents, CancellationToken ct)
  {
    Notification notification = notificationCreatedEvent.Notification;

    List<UserId> userIds = new();

    if (notification.TargetScope == NotificationTargetScopeEnum.Broadcast)
    {
      userIds = await _userQueryService.GetAllUserIds(ct);
    }
    else if (notification.TargetScope == NotificationTargetScopeEnum.Role && notification.TargetRoleId != null)
    {
      userIds = await _userQueryService.GetUserIdsWithRole((RoleId)notification.TargetRoleId!, ct);
    }
    else if (notification.TargetScope == NotificationTargetScopeEnum.User && notification.TargetUserId != null)
    {
      userIds.Add((UserId)notification.TargetUserId);
    }

    foreach (var userId in userIds)
      notification.AddRecipient(userId);

    await _notificationRepository.Create(notification, ct);
  }
}
