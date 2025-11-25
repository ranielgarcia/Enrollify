using Enrollify.Core.RoleAggregate;
using Enrollify.Core.RoomAggregate;
using Enrollify.Core.RoomTypeAggregate;
using Enrollify.Core.UserAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config;

// RoomType
[EfCoreConverter<RoomTypeId>]
[EfCoreConverter<RoomTypeName>]
// Room
[EfCoreConverter<RoomId>]
[EfCoreConverter<RoomName>]
// Role
[EfCoreConverter<RoleId>]
[EfCoreConverter<RoleName>]
[EfCoreConverter<RoleDescription>]
// User
[EfCoreConverter<UserEmail>]
[EfCoreConverter<UserFirstName>]
[EfCoreConverter<UserLastName>]
[EfCoreConverter<UserId>]
internal partial class VogenEfCoreConverters;
