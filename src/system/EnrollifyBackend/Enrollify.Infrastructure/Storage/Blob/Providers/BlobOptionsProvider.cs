using System.Collections.Concurrent;
using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Models;

namespace Enrollify.Infrastructure.Storage.Blob.Providers;

public interface IBlobOptionsProvider<TSettings>
    where TSettings : IStorageSettings
{
    BlobOptions GetBlobOptions<T>() where T : class;
    BlobOptions GetBlobOptions(Type type);
}

public class BlobOptionsProvider<TSettings>
    : IBlobOptionsProvider<TSettings>
    where TSettings : IStorageSettings
{
    private readonly IContainerNameProvider<TSettings> _containerNameProvider;
    private static readonly ConcurrentDictionary<Type, BlobOptions> BlobOptionsMap = new();

    public BlobOptionsProvider(IContainerNameProvider<TSettings> containerNameProvider)
    {
        _containerNameProvider = containerNameProvider;
    }

    public BlobOptions GetBlobOptions<T>() where T : class
    {
        return GetBlobOptions(typeof(T));
    }

    public BlobOptions GetBlobOptions(Type type)
    {
        return BlobOptionsMap.GetOrAdd(type, t =>
            new BlobOptions(
                t,
                _containerNameProvider.GetContainerName(t),
                blockSize: 8,
                concurrentCount: Environment.ProcessorCount));
    }
}
