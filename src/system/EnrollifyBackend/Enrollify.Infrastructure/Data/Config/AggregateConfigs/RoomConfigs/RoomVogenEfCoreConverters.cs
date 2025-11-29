using Enrollify.Core.Aggregates.RoomAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomConfigs;

[EfCoreConverter<RoomId>]
[EfCoreConverter<RoomName>]
[EfCoreConverter<RoomStudentCapacity>]
internal partial class RoomVogenEfCoreConverters;
