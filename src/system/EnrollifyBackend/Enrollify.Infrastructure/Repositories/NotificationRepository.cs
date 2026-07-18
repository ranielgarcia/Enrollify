using Ardalis.Result;
using Dapper;
using Enrollify.Application.Features.Notifications;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Enrollify.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly ILogger<NotificationRepository> _logger;

  public NotificationRepository(EnrollifyDbContext dbContext, IDbConnectionFactory connectionFactory, ILogger<NotificationRepository> logger)
  {
    _dbContext = dbContext;
    _connectionFactory = connectionFactory;
    _logger = logger;
  }

  public async Task<Result> BulkCreate(Notification[] notifications, CancellationToken cancellationToken)
  {
    try
    {
      await _dbContext.Notifications.AddRangeAsync(notifications, cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to save notification");
      return Result.Error($"An error occurred while creating the notification: {ex.Message}");
    }
  }

  public async Task<Result<NotificationId>> Update(Notification notification, CancellationToken cancellationToken)
  {
    try
    {
      _dbContext.Notifications.Update(notification);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success(notification.Id);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to update notification");
      return Result.Error($"An error occurred while updating the notification: {ex.Message}");
    }
  }

  public async Task<Result> Delete(NotificationId id, CancellationToken cancellationToken)
  {
    try
    {
      var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
      if (notification == null)
      {
        return Result.NotFound($"Notification with ID {id.Value} not found.");
      }

      _dbContext.Notifications.Remove(notification);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to delete notification");
      return Result.Error($"An error occurred while deleting the notification: {ex.Message}");
    }
  }

  public async Task<Result> MarkAllNotificationsAsReadForUser(UserId userId, CancellationToken cancellationToken)
  {
    try
    {
      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(cancellationToken);

      string query = @"UPDATE NR
          SET NR.IsRead=1, NR.ReadAt = SYSUTCDATETIME()
          FROM NotificationRecipients NR
          JOIN Notifications N ON N.Id = NR.NotificationId
          WHERE NR.UserId = @UserId AND N.ExpiresAt >= SYSUTCDATETIME()";

      var command = new CommandDefinition(
        commandText: query,
        parameters: new { UserId = userId.Value },
        cancellationToken: cancellationToken
      );

      await conn.ExecuteAsync(command);

      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to mark notifications as read");
      return Result.Error($"An error occurred while marking notifications as read: {ex.Message}");
    }
  }

  public async Task<Result> MarkAllNotificationsAsDismissedForUser(UserId userId, CancellationToken cancellationToken)
  {
    try
    {
      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(cancellationToken);

      string query = @"UPDATE NR
          SET NR.IsDismissed=1, NR.DismissedAt = SYSUTCDATETIME()
          FROM NotificationRecipients NR
          JOIN Notifications N ON N.Id = NR.NotificationId
          WHERE NR.UserId = @UserId AND N.ExpiresAt >= SYSUTCDATETIME()";

      var command = new CommandDefinition(
        commandText: query,
        parameters: new { UserId = userId.Value },
        cancellationToken: cancellationToken
      );

      await conn.ExecuteAsync(command);

      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unable to mark notifications as dismissed");
      return Result.Error($"An error occurred while marking notifications as dismissed: {ex.Message}");
    }
  }
}
