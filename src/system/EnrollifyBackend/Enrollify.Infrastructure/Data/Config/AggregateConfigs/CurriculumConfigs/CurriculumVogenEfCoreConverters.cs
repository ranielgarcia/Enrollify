using Enrollify.Core.Aggregates.CurriculumAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CurriculumConfigs;

[EfCoreConverter<CurriculumId>]
[EfCoreConverter<CurriculumSubjectId>]
public partial class CurriculumVogenEfCoreConverters;
