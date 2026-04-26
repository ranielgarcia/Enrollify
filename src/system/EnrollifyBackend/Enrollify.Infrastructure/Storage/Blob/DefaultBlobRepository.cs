using Azure;
using Azure.Storage;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Enrollify.Application.Storage;
using Enrollify.Application.Storage.Blob;
using Enrollify.Core.ValueObjects.Storage;
using Enrollify.Infrastructure.Storage.Blob.Constants;
using Enrollify.Infrastructure.Storage.Blob.Models;
using Enrollify.Infrastructure.Storage.Blob.Providers;

namespace Enrollify.Infrastructure.Storage.Blob;

public class DefaultBlobRepository<TSettings, TEntity>
    : IBlobRepository<TSettings, TEntity>
    where TSettings : IStorageSettings
    where TEntity : class
{
    private readonly ILogger<DefaultBlobRepository<TSettings, TEntity>> _logger;
    private readonly IBlobContainerProvider<TSettings, TEntity> _containerProvider;
    private readonly BlobOptions _blobOptions;

    public DefaultBlobRepository(
        ILogger<DefaultBlobRepository<TSettings, TEntity>> logger,
        IBlobContainerProvider<TSettings, TEntity> containerProvider,
        IBlobOptionsProvider<TSettings> optionsProvider)
    {
        _logger = logger;
        _containerProvider = containerProvider;
        _blobOptions = optionsProvider.GetBlobOptions<TEntity>();
    }


    public async Task EnsureContainerExists(CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<string> UploadFileAsync(
        FileName fileName,
        Stream content,
        string[]? subfolders = null,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        StorageTransferOptions transferOptions = new()
        {
            MaximumTransferSize = _blobOptions.BlockSize * StorageConstants.Blob.Size.MegaBytes,
            MaximumConcurrency = _blobOptions.ConcurrentCount
        };

        var uploadOptions = new BlobUploadOptions
        {
            TransferOptions = transferOptions,
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType ?? ResolveContentType(fileName)
            }
        };

        if (!overwrite)
        {
            uploadOptions.Conditions = new BlobRequestConditions { IfNoneMatch = new ETag("*") };
        }

        await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

        return blobPath;
    }

    public async Task<StoredFile?> GetFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        try
        {
            var response = await blobClient.DownloadStreamingAsync( cancellationToken: cancellationToken);
            var details = response.Value.Details;

            return new StoredFile
            {
                FileName = fileName,
                ContentType = details.ContentType ?? "application/octet-stream",
                Content = response.Value.Content,
                Size = details.ContentLength,
                LastModified = details.LastModified
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<StoredFile>> GetFilesAsync(
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var prefix = BuildPrefix(subfolders);
        var files = new List<StoredFile>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var response = await blobClient.DownloadContentAsync(cancellationToken);

            files.Add(new StoredFile
            {
                FileName = FileName.From(Path.GetFileName(blobItem.Name)),
                ContentType = response.Value.Details.ContentType ?? "application/octet-stream",
                Content = response.Value.Content.ToStream(),
                Size = response.Value.Details.ContentLength,
                LastModified = response.Value.Details.LastModified
            });
        }

        return files;
    }

    public async Task<bool> DeleteFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    public async Task<bool> ArchiveFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        try
        {
            await blobClient.SetAccessTierAsync(AccessTier.Archive, cancellationToken: cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<bool> UnarchiveFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        bool highPriority = false,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        try
        {
            var priority = highPriority
                ? RehydratePriority.High
                : RehydratePriority.Standard;

            await blobClient.SetAccessTierAsync(
                AccessTier.Hot,
                rehydratePriority: priority,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<FileMetadata>> ListFilesAsync(
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var prefix = BuildPrefix(subfolders);
        var files = new List<FileMetadata>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
        {
            files.Add(new FileMetadata
            {
                FileName = FileName.From(Path.GetFileName(blobItem.Name)),
                Path = blobItem.Name,
                ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                Size = blobItem.Properties.ContentLength ?? 0,
                LastModified = blobItem.Properties.LastModified
            });
        }

        return files;
    }

    public async Task<bool> FileExistsAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        try
        {
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return new FileMetadata
            {
                FileName = fileName,
                Path = blobPath,
                ContentType = properties.Value.ContentType ?? "application/octet-stream",
                Size = properties.Value.ContentLength,
                LastModified = properties.Value.LastModified
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<string?> GetFileUrlAsync(
        FileName fileName,
        string[]? subfolders = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = await _containerProvider.GetBlobContainerClient();
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        if (!blobClient.CanGenerateSasUri)
        {
            return null;
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1))
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    private static string BuildBlobPath(string[]? subfolders, FileName fileName)
    {
        if (subfolders is { Length: > 0 })
        {
            return string.Join("/", subfolders) + "/" + fileName.Value;
        }

        return fileName.Value;
    }

    private static string BuildPrefix(string[]? subfolders)
    {
        if (subfolders is { Length: > 0 })
        {
            return string.Join("/", subfolders) + "/";
        }

        return string.Empty;
    }

    private static string ResolveContentType(FileName fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName.Value)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            _ => "application/octet-stream"
        };
    }

}
