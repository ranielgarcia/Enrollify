using System;
using System.Collections.Generic;
using System.Text;
using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Builders;

namespace Enrollify.Infrastructure.Storage.Blob.Models;


/// <summary>
/// Top-level options object registered into IOptions&lt;BlobRepositoryOptions&lt;TSettings&gt;&gt;.
/// Holds the storage account settings and all blob entity registrations.
/// </summary>
public class BlobRepositoryOptions<TSettings>
    where TSettings : IStorageSettings
{
    public IStorageSettings Settings { get; set; } = default!;

    public IBlobBuilder<TSettings> BlobBuilder { get; } = new BlobBuilder<TSettings>();

    internal IReadOnlyList<BlobOptionsBuilder<TSettings>> BlobOptions => BlobBuilder.Options;

    internal BlobOptionsBuilder<TSettings>? GetBlobOptions<T>() where T : class
    {
        return GetBlobOptions(typeof(T));
    }

    internal BlobOptionsBuilder<TSettings>? GetBlobOptions(Type type)
    {
        return BlobOptions.FirstOrDefault(bo => bo.Type == type);
    }
}
