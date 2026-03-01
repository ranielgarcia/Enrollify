using Enrollify.Core.Aggregates.TeacherAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.TeacherConfigs;

[EfCoreConverter<TeacherId>]
[EfCoreConverter<TeacherEmail>]
public partial class TeacherVogenEfCoreConverters;
