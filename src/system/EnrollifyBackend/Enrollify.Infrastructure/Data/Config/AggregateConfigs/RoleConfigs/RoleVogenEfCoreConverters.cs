using Enrollify.Core.Aggregates.RoleAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoleConfigs;


[EfCoreConverter<RoleId>]
[EfCoreConverter<RoleName>]
[EfCoreConverter<RoleDescription>]
internal partial class RoleVogenEfCoreConverters;