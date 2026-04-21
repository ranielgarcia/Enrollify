using Enrollify.Core.ValueObjects.Storage;
using Vogen;

namespace Enrollify.Infrastructure.Data.Config;

[EfCoreConverter<FileName>]
[EfCoreConverter<SubFolder>]
public partial class SharedValueObjectsVogenEfCoreConverters;
