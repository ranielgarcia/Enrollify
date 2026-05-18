using Enrollify.Core.ValueObjects;
using Enrollify.Core.ValueObjects.Storage;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config;

[EfCoreConverter<FileName>]
[EfCoreConverter<SubFolder>]
[EfCoreConverter<YearLevel>]
[EfCoreConverter<TermNumber>]
[EfCoreConverter<Year>]
public partial class SharedValueObjectsVogenEfCoreConverters;
