using Enrollify.Core.Aggregates.PermissionsAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RolePermissionConfigs;

[EfCoreConverter<PermissionId>]
[EfCoreConverter<PermissionName>]
[EfCoreConverter<PermissionResource>]
[EfCoreConverter<PermissionAction>]
[EfCoreConverter<PermissionDescription>]
internal partial class PermissionVogenEfCoreConverters;