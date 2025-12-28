using Enrollify.Core.Aggregates.CourseAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CourseConfigs;

[EfCoreConverter<CourseId>]
[EfCoreConverter<CourseCode>]
public partial class CourseVogenEfCoreConverters;
