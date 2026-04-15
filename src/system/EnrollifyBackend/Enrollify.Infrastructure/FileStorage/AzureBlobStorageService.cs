using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Enrollify.Core.Services.FileStorage;

namespace Enrollify.Infrastructure.FileStorage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadFileAsync(
        string container,
        string fileName,
        Stream content,
        string[]? subfolders = null,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var uploadOptions = new BlobUploadOptions
        {
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

        return $"{container}/{blobPath}";
    }

    public async Task<StoredFile?> GetFileAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        try
        {
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
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
        string container,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var prefix = BuildPrefix(subfolders);
        var files = new List<StoredFile>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var response = await blobClient.DownloadContentAsync(cancellationToken);

            files.Add(new StoredFile
            {
                FileName = System.IO.Path.GetFileName(blobItem.Name),
                ContentType = response.Value.Details.ContentType ?? "application/octet-stream",
                Content = response.Value.Content.ToStream(),
                Size = response.Value.Details.ContentLength,
                LastModified = response.Value.Details.LastModified
            });
        }

        return files;
    }

    public async Task<bool> DeleteFileAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    public async Task<bool> ArchiveFileAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
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
        string container,
        string fileName,
        string[]? subfolders = null,
        bool highPriority = false,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
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
        string container,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var prefix = BuildPrefix(subfolders);
        var files = new List<FileMetadata>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
        {
            files.Add(new FileMetadata
            {
                FileName = System.IO.Path.GetFileName(blobItem.Name),
                Path = blobItem.Name,
                ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                Size = blobItem.Properties.ContentLength ?? 0,
                LastModified = blobItem.Properties.LastModified
            });
        }

        return files;
    }

    public async Task<bool> FileExistsAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var blobPath = BuildBlobPath(subfolders, fileName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
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
        string container,
        string fileName,
        string[]? subfolders = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
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
            BlobContainerName = container,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1))
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    private static string BuildBlobPath(string[]? subfolders, string fileName)
    {
        if (subfolders is { Length: > 0 })
        {
            return string.Join("/", subfolders) + "/" + fileName;
        }

        return fileName;
    }

    private static string BuildPrefix(string[]? subfolders)
    {
        if (subfolders is { Length: > 0 })
        {
            return string.Join("/", subfolders) + "/";
        }

        return string.Empty;
    }

    private static string ResolveContentType(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
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
