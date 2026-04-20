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
        string fileName,
        Stream content,
        string[]? subfolders = null,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    Task<StoredFile?> GetFileAsync(
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetFilesAsync(
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveFileAsync(
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> UnarchiveFileAsync(
        string fileName,
        string[]? subfolders = null,
        bool highPriority = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileMetadata>> ListFilesAsync(
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<bool> FileExistsAsync(
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    Task<FileMetadata?> GetFileMetadataAsync(
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);
    Task<string?> GetFileUrlAsync(
        string fileName,
        string[]? subfolders = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);
}
