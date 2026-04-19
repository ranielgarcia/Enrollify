using Enrollify.Application.Constants;
using Enrollify.Application.Services.FileStorage;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers;

public interface ITeacherPhotoStorageService
{
    Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);
    Task<bool> DeletePhotoAsync(TeacherId teacherId, string fileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredFile>> GetAllPhotosAsync(TeacherId teacherId, CancellationToken cancellationToken = default);
    Task<StoredFile?> GetPhotoAsync(TeacherId teacherId, string fileName, CancellationToken cancellationToken = default);
    Task<FileMetadata?> GetPhotoMetadataAsync(TeacherId teacherId, string fileName, CancellationToken cancellationToken = default);
    Task<string?> GetPhotoUrlAsync(TeacherId teacherId, string fileName, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileMetadata>> ListPhotosAsync(TeacherId teacherId, CancellationToken cancellationToken = default);
    Task<bool> PhotoExistsAsync(TeacherId teacherId, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadPhotoAsync(TeacherId teacherId, string fileName, Stream content, string? contentType = null, bool overwrite = true, CancellationToken cancellationToken = default);
}

public class TeacherPhotoStorageService : ITeacherPhotoStorageService
{
    private static readonly string Container = FileStorageConstants.TeacherFilesContainerName;
    private static readonly string PhotosSubfolder = "photos";

    private readonly IFileStorageService _fileStorageService;

    public TeacherPhotoStorageService(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        await _fileStorageService.EnsureContainerExists(Container, cancellationToken);
    }

    public async Task<string> UploadPhotoAsync(
        TeacherId teacherId,
        string fileName,
        Stream content,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.UploadFileAsync(
            Container,
            fileName,
            content,
            subfolders: GetSubfolders(teacherId),
            contentType: contentType,
            overwrite: overwrite,
            cancellationToken: cancellationToken);
    }

    public async Task<StoredFile?> GetPhotoAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.GetFileAsync(
            Container,
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetAllPhotosAsync(
        TeacherId teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.GetFilesAsync(
            Container,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> DeletePhotoAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.DeleteFileAsync(
            Container,
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> PhotoExistsAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.FileExistsAsync(
            Container,
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<FileMetadata>> ListPhotosAsync(
        TeacherId teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.ListFilesAsync(
            Container,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<FileMetadata?> GetPhotoMetadataAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.GetFileMetadataAsync(
            Container,
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<string?> GetPhotoUrlAsync(
        TeacherId teacherId,
        string fileName,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageService.GetFileUrlAsync(
            Container,
            fileName,
            subfolders: GetSubfolders(teacherId),
            expiry: expiry,
            cancellationToken: cancellationToken);
    }

    private static string[] GetSubfolders(TeacherId teacherId) =>
        [teacherId.Value.ToString(), PhotosSubfolder];
}
