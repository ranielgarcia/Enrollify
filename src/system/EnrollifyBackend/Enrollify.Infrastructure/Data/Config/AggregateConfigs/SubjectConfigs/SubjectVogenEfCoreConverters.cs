using Enrollify.Core.Aggregates.SubjectAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.SubjectConfigs;

[EfCoreConverter<SubjectId>]
[EfCoreConverter<SubjectCode>]
internal partial class SubjectVogenEfCoreConverters;