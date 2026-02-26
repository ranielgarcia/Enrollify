using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.SubjectEquivalenceGroupConfigs;

[EfCoreConverter<SubjectEquivalenceGroupId>]
[EfCoreConverter<SubjectEquivalenceId>]
public partial class SubjectEquivalenceGroupVogenEfCoreConverters;
