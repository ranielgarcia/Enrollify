using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.AcademicYearConfigs;

[EfCoreConverter<AcademicYearId>]
[EfCoreConverter<AcademicStartDate>]
[EfCoreConverter<AcademicEndDate>]
internal partial class AcademicYearVogenEfCoreConverters;
