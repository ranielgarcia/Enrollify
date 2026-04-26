using Azure.Storage.Blobs;
using Enrollify.Application.Storage;
using Enrollify.Infrastructure.Storage.Blob.Models;

namespace Enrollify.Infrastructure.Storage.Blob.Providers;


/// <summary>
/// Provides access to the Azure BlobServiceClient for a given storage account.
/// </summary>
public interface IBlobServiceClientProvider<TSettings>
    where TSettings : IStorageSettings
{
    /// <summary>
    /// Executes an operation against the BlobServiceClient.
    /// The provider manages client lifetime and authentication.
    /// </summary>
    Task<T> UseServiceClient<T>(Func<BlobServiceClient, Task<T>> operation);
}

/// <summary>
/// Creates and caches a BlobServiceClient for the storage account
/// identified by TSettings.
/// </summary>
public class BlobServiceClientProvider<TSettings>
    : IBlobServiceClientProvider<TSettings>
    where TSettings : IStorageSettings
{
    private readonly Lazy<BlobServiceClient> _client;

    public BlobServiceClientProvider(IOptions<BlobRepositoryOptions<TSettings>> options)
    {
        ArgumentNullException.ThrowIfNull(options?.Value?.Settings);

        var connectionString = options.Value.Settings.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string for {typeof(TSettings).Name} is not configured.");
        }

        // TODO: Remove explicit ServiceVersion once Azurite supports the 2026-02-06 API version
        _client = new Lazy<BlobServiceClient>(() => new BlobServiceClient(connectionString, new BlobClientOptions(BlobClientOptions.ServiceVersion.V2025_01_05)));
    }

    public Task<T> UseServiceClient<T>(Func<BlobServiceClient, Task<T>> operation)
    {
        return operation(_client.Value);
    }
}
