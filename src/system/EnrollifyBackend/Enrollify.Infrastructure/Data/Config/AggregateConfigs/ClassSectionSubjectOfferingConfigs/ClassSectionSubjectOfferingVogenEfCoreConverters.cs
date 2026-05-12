using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionSubjectOfferingConfigs;

[EfCoreConverter<ClassSectionSubjectOfferingId>]
[EfCoreConverter<ClassScheduleId>]
internal partial class ClassSectionSubjectOfferingVogenEfCoreConverters
{
}
