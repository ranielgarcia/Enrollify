using Enrollify.Core.Aggregates.NotificationAggregate;

namespace Enrollify.Core.Services.NotificationServices.Models;

public record NotificationCreatedEvent (Notification Notification);
