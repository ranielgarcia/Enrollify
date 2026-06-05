using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConfigs;

[EfCoreConverter<ClassSectionId>]
[EfCoreConverter<SectionCode>]
[EfCoreConverter<ClassSectionEnrollmentEligibilityValidationMessageId>]
internal partial class ClassSectionVogenEfCoreConverters
{
}
