using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Models;

namespace Enrollify.Infrastructure.Storage.Blob.Providers;

public interface IContainerNameProvider<TSettings>
    where TSettings : IStorageSettings
{
    string GetContainerName<T>() where T : class;
    string GetContainerName(Type type);
}

public class ContainerNameProvider<TSettings>
    : IContainerNameProvider<TSettings>
    where TSettings : IStorageSettings
{
    private readonly IOptions<BlobRepositoryOptions<TSettings>> _options;

    public ContainerNameProvider(IOptions<BlobRepositoryOptions<TSettings>> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string GetContainerName<T>() where T : class
    {
        return GetContainerName(typeof(T));
    }

    public string GetContainerName(Type type)
    {
        var optionsBuilder = _options.Value.GetBlobOptions(type);
        var containerName = optionsBuilder?.ContainerName;

         return containerName
             ?? throw new InvalidOperationException(
                 $"No blob container configured for '{type.Name}'. Register with BlobBuilder.Configure<{type.Name}>().");
    }
}
