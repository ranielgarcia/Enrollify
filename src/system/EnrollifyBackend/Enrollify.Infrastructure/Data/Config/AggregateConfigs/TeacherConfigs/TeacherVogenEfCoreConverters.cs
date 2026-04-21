using Enrollify.Core.Aggregates.TeacherAggregate;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.TeacherConfigs;

[EfCoreConverter<TeacherId>]
[EfCoreConverter<TeacherEmail>]
[EfCoreConverter<TeacherPhoneNumber>]
[EfCoreConverter<TeacherIdentifier>]
public partial class TeacherVogenEfCoreConverters;
