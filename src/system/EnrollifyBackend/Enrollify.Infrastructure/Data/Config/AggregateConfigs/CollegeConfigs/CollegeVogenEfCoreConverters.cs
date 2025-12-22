using Enrollify.Core.Aggregates.CollegeAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CollegeConfigs;

[EfCoreConverter<CollegeId>]
[EfCoreConverter<CollegeCode>]
internal partial class CollegeVogenEfCoreConverters;