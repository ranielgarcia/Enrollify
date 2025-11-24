using Enrollify.CleanArchPOC.Core.ContributorAggregate;
using Vogen;

namespace Enrollify.CleanArchPOC.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
internal partial class VogenEfCoreConverters;
