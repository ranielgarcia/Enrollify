using Enrollify.Core.Aggregates.NotificationAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.NotificationConfigs;

[EfCoreConverter<NotificationId>]
[EfCoreConverter<NotificationRecipientId>]
public partial class NotificationVogenEfCoreConverters;
