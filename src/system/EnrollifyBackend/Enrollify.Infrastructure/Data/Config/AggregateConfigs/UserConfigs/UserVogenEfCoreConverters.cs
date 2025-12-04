using Enrollify.Core.Aggregates.UserAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.UserConfigs;

[EfCoreConverter<UserId>]
[EfCoreConverter<UserEmail>]
internal partial class UserVogenEfCoreConverters;
