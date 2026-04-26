using Enrollify.Core.ValueObjects.Storage;

namespace Enrollify.Application.Storage.Blob;

/// <summary>
/// Generic blob repository. TSettings selects the storage account,
/// TEntity selects the container.
/// </summary>
public interface IBlobRepository<TSettings, TEntity>
    where TSettings : IStorageSettings
    where TEntity : class
{
    Task EnsureContainerExists(CancellationToken cancellationToken = default);

    Task<string> UploadFileAsync(
        FileName fileName,
        Stream content,
        string[]? subfolders = null,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    Task<StoredFile?> GetFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetFilesAsync(
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> UnarchiveFileAsync(
        FileName fileName,
        string[]? subfolders = null,
        bool highPriority = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileMetadata>> ListFilesAsync(
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> FileExistsAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<FileMetadata?> GetFileMetadataAsync(
        FileName fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);
    Task<string?> GetFileUrlAsync(
        FileName fileName,
        string[]? subfolders = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);
}
