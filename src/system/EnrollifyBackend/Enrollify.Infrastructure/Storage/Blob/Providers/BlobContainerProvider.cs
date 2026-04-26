using Azure.Storage.Blobs;
using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Services;

namespace Enrollify.Infrastructure.Storage.Blob.Providers;

public interface IBlobContainerProvider<TSettings, TEntity>
    where TSettings : IStorageSettings
    where TEntity : class
{
    Task<BlobContainerClient> GetBlobContainerClient();
}

public class BlobContainerProvider<TSettings, TEntity>
    : IBlobContainerProvider<TSettings, TEntity>
    where TSettings : IStorageSettings
    where TEntity : class
{
    private readonly Lazy<Task<BlobContainerClient>> _lazyClient;

    public BlobContainerProvider(IBlobContainerService<TSettings> containerService)
    {
        _lazyClient = new Lazy<Task<BlobContainerClient>>(
            async () => await containerService.GetBlobContainerClient<TEntity>());
    }

    public Task<BlobContainerClient> GetBlobContainerClient()
    {
        return _lazyClient.Value;
    }
}
