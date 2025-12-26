using Enrollify.Core.Aggregates.DepartmentAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.DepartmentConfigs;

[EfCoreConverter<DepartmentId>]
[EfCoreConverter<DepartmentCode>]
public partial class DepartmentVogenEfCoreConverters;