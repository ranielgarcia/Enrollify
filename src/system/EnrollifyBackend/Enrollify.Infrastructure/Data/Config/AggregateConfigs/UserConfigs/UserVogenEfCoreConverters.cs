using Enrollify.Core.Aggregates.UserAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.UserConfigs;

[EfCoreConverter<UserId>]
[EfCoreConverter<UserRoleAssignmentId>]
internal partial class UserVogenEfCoreConverters;
