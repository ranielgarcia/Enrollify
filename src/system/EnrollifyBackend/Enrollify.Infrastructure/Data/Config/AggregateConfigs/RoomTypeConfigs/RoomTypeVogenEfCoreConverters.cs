using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomTypeConfigs;

[EfCoreConverter<RoomTypeId>]
[EfCoreConverter<RoomTypeName>]
internal partial class RoomTypeVogenEfCoreConverters;
