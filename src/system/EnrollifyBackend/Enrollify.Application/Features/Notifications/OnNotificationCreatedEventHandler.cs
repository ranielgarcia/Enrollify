using Enrollify.Application.Features.Notifications.DTOs;
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
  private readonly IRealTimeNotificationSender _realTimeNotificationSender;
  private readonly ILogger<OnNotificationCreatedEventHandler> _logger;

  public OnNotificationCreatedEventHandler(
    INotificationRepository notificationRepository,
    IUserQueryService userQueryService,
    IRealTimeNotificationSender realTimeNotificationSender,
    ILogger<OnNotificationCreatedEventHandler> logger)
  {
    _notificationRepository = notificationRepository;
    _userQueryService = userQueryService;
    _realTimeNotificationSender = realTimeNotificationSender;
    _logger = logger;
  }

  public async Task Handle(NotificationCreatedEvent[] notificationCreatedEvents, CancellationToken ct)
  {
    // TODO: Cache roles and users
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

      _logger.LogDebug("Notification {NotificationId} message: {Message} created with {RecipientCount} recipients.", notification.Id, notification.Message, notification.Recipients.Count);
      notifications.Add(notification);
    }

    var bulkResult = await _notificationRepository.BulkCreate(notifications.ToArray(), ct);

    if (bulkResult.IsSuccess)
    {
      // Push a real-time event to each recipient's active connection(s) now that the
      // notification (and its recipients) have been successfully persisted.
      foreach (var notification in notifications)
      {
        var recipientUserIds = notification.Recipients.Select(r => r.UserId).ToArray();
        if (recipientUserIds.Length == 0)
          continue;

        await _realTimeNotificationSender.SendToUsersAsync(recipientUserIds, NotificationDto.FromEntity(notification), ct);
      }
    }

  }
}
