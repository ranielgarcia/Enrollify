using Enrollify.Core.Aggregates.CurriculumAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CurriculumConfigs;

[EfCoreConverter<CurriculaId>]
[EfCoreConverter<CurriculumSubjectId>]
internal partial class CurriculaVogenEfCoreConverters;
