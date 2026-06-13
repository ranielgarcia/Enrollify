using Enrollify.Core.Aggregates.ClassSectionConflictAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConflictConfigs;

[EfCoreConverter<ClassSectionConflictId>]
[EfCoreConverter<ClassSectionConflictAffectedOfferingId>]
internal partial class ClassSectionConflictVogenEfCoreConverters;
