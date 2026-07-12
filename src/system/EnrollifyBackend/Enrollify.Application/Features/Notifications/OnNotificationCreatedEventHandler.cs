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
    RoleId[] targetRoleIds = notificationCreatedEvents
      .Where(e => e.Notification.TargetScope == NotificationTargetScopeEnum.Role)
      .Select(e => e.Notification.TargetRoleId)
      .Where(id => id != null).Select(id => (RoleId)id!).Distinct().ToArray();
    var allUserIds = await _userQueryService.GetAllUserIds(ct);

    List<UserIdRoleId> userIdsWithRoles = await _userQueryService.GetUserIdsWithRoles(targetRoleIds, ct);
    var userIdsByRole = userIdsWithRoles
      .GroupBy(x => x.RoleId)
      .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).ToList());

    var notifications = new List<Notification>();
    foreach (NotificationCreatedEvent notificationCreatedEvent in notificationCreatedEvents)
    {
      List<UserId> userIds = new();

      Notification notification = notificationCreatedEvent.Notification;

      if (notification.TargetScope == NotificationTargetScopeEnum.Broadcast)
      {
        userIds = allUserIds;
      }
      else if (notification.TargetScope == NotificationTargetScopeEnum.Role && notification.TargetRoleId != null)
      {
        userIds = userIdsByRole.TryGetValue((RoleId)notification.TargetRoleId, out var ids) ? ids : new List<UserId>();
      }
      else if (notification.TargetScope == NotificationTargetScopeEnum.User && notification.TargetUserId != null)
      {
        userIds.Add((UserId)notification.TargetUserId);
      }

      foreach (var userId in userIds)
        notification.AddRecipient(userId);

      notifications.Add(notification);
    }

    await _notificationRepository.BulkCreate(notifications.ToArray(), ct);
  }
}
