using Enrollify.Core.RoomAggregate;
using Enrollify.Core.RoomTypeAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config;

[EfCoreConverter<RoomTypeId>]
[EfCoreConverter<RoomTypeName>]
[EfCoreConverter<RoomId>]
[EfCoreConverter<RoomName>]
internal partial class VogenEfCoreConverters;
