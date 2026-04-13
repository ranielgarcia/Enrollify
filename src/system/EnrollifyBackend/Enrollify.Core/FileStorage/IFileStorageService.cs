namespace Enrollify.Core.FileStorage;

/// <summary>
/// Provides a generic abstraction for file storage operations across the application.
/// Supports uploading, retrieving, deleting, archiving, and listing files
/// organized by container and optional subfolder paths.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to the specified container and optional subfolder path.
    /// </summary>
    /// <returns>The URI of the uploaded file.</returns>
    Task<string> UploadFileAsync(
        string container,
        string fileName,
        Stream content,
        string[]? subfolders = null,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific file by container, filename, and optional subfolder path.
    /// The caller is responsible for disposing the <see cref="StoredFile.Content"/> stream.
    /// </summary>
    /// <returns>The file with its content stream, or <c>null</c> if not found.</returns>
    Task<StoredFile?> GetFileAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all files (including content) from the specified container and optional subfolder path.
    /// Use <see cref="ListFilesAsync"/> for metadata-only listing when content is not needed.
    /// </summary>
    Task<IReadOnlyList<StoredFile>> GetFilesAsync(
        string container,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific file from the specified container and optional subfolder path.
    /// </summary>
    /// <returns><c>true</c> if the file was deleted; <c>false</c> if it was not found.</returns>
    Task<bool> DeleteFileAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a specific file, moving it to a cold/archive storage tier.
    /// Archived files remain accessible but may require rehydration before download.
    /// </summary>
    /// <returns><c>true</c> if the file was archived successfully; <c>false</c> if not found.</returns>
    Task<bool> ArchiveFileAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists file metadata from the specified container and optional subfolder path
    /// without downloading file content.
    /// </summary>
    Task<IReadOnlyList<FileMetadata>> ListFilesAsync(
        string container,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a file exists at the specified container, filename, and optional subfolder path.
    /// </summary>
    Task<bool> FileExistsAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves metadata for a specific file without downloading its content.
    /// </summary>
    /// <returns>The file metadata, or <c>null</c> if the file was not found.</returns>
    Task<FileMetadata?> GetFileMetadataAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a pre-signed, time-limited URL for direct read access to a file.
    /// Useful for serving files to clients without proxying through the API.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="subfolders">Optional subfolder path segments.</param>
    /// <param name="expiry">URL validity duration. Defaults to 1 hour if not specified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A pre-signed URL, or <c>null</c> if the file was not found or URL generation is not supported.</returns>
    Task<string?> GetFileUrlAsync(
        string container,
        string fileName,
        string[]? subfolders = null,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);
}
