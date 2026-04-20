using Azure.Storage.Blobs;
using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Providers;

namespace Enrollify.Infrastructure.Storage.Blob.Services;

public interface IBlobContainerService<TSettings>
    where TSettings : IStorageSettings
{
    Task<BlobContainerClient> GetBlobContainerClient<T>() where T : class;
}

public class BlobContainerService<TSettings>
    : IBlobContainerService<TSettings>
    where TSettings : IStorageSettings
{
    private readonly ILogger<BlobContainerService<TSettings>> _logger;
    private readonly IBlobOptionsProvider<TSettings> _blobOptionsProvider;
    private readonly IBlobServiceClientProvider<TSettings> _serviceClientProvider;

    public BlobContainerService(
        ILogger<BlobContainerService<TSettings>> logger,
        IBlobOptionsProvider<TSettings> blobOptionsProvider,
        IBlobServiceClientProvider<TSettings> serviceClientProvider)
    {
        _logger = logger;
        _blobOptionsProvider = blobOptionsProvider;
        _serviceClientProvider = serviceClientProvider;
    }

    public Task<BlobContainerClient> GetBlobContainerClient<T>() where T : class
    {
        return GetBlobContainerClient(typeof(T));
    }

    private async Task<BlobContainerClient> GetBlobContainerClient(Type type)
    {
        try
        {
            var blobOptions = _blobOptionsProvider.GetBlobOptions(type);
            var client = await _serviceClientProvider.UseServiceClient(client =>
                Task.FromResult(client.GetBlobContainerClient(blobOptions.ContainerName)));

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get blob container client for {TypeName}. {Message}",
                type.Name, ex.Message);
            throw;
        }
    }
}
