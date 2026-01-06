using Enrollify.Core.Aggregates.CurriculaAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CurriculaConfigs;

[EfCoreConverter<CurriculaId>]
[EfCoreConverter<CurriculumSubjectId>]
internal partial class CurriculaVogenEfCoreConverters;
