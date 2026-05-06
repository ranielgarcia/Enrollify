using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.AcademicYearConfigs;

[EfCoreConverter<AcademicYearId>]
[EfCoreConverter<AcademicYearStartDate>]
[EfCoreConverter<AcademicYearEndDate>]
[EfCoreConverter<AcademicTermId>]
[EfCoreConverter<AcademicTermNumber>]
[EfCoreConverter<AcademicTermStartDate>]
[EfCoreConverter<AcademicTermEndDate>]
internal partial class AcademicYearVogenEfCoreConverters;
