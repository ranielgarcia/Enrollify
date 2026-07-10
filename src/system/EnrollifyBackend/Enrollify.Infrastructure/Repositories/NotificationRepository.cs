using Ardalis.Result;
using Enrollify.Application.Features.Notifications;
using Enrollify.Core.Aggregates.NotificationAggregate;
using Enrollify.Infrastructure.Data;

namespace Enrollify.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
  private readonly EnrollifyDbContext _dbContext;
  private readonly ILogger<NotificationRepository> _logger;

  public NotificationRepository(EnrollifyDbContext dbContext, ILogger<NotificationRepository> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task<Result<NotificationId>> Create(Notification notification, CancellationToken cancellationToken)
  {
    try
    {
      await _dbContext.Notifications.AddAsync(notification, cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);
      return Result.Success(notification.Id);
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
}
