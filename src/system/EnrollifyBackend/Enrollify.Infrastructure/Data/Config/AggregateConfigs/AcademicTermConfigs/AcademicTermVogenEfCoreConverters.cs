using Enrollify.Core.Aggregates.AcademicTermAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.AcademicTermConfigs;

[EfCoreConverter<AcademicTermId>]
[EfCoreConverter<AcademicTermNumber>]
internal partial class AcademicTermVogenEfCoreConverters
{
}
