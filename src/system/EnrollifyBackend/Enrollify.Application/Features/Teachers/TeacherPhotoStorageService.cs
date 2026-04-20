using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Storage;
using Enrollify.Application.Storage.Blob;
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
    private static readonly string PhotosSubfolder = "photos";
    private readonly IBlobRepository<DefaultStorageSettings, TeacherBlobConfig> _blobRepository;

    public TeacherPhotoStorageService(IBlobRepository<DefaultStorageSettings, TeacherBlobConfig> blobRepository)
    {
        _blobRepository = blobRepository;
    }

    public async Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        await _blobRepository.EnsureContainerExists(cancellationToken);
    }

    public async Task<string> UploadPhotoAsync(
        TeacherId teacherId,
        string fileName,
        Stream content,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.UploadFileAsync(
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
        return await _blobRepository.GetFileAsync(
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetAllPhotosAsync(
        TeacherId teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.GetFilesAsync(
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> DeletePhotoAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.DeleteFileAsync(
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> PhotoExistsAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.FileExistsAsync(
            fileName,
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<FileMetadata>> ListPhotosAsync(
        TeacherId teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.ListFilesAsync(
            subfolders: GetSubfolders(teacherId),
            cancellationToken: cancellationToken);
    }

    public async Task<FileMetadata?> GetPhotoMetadataAsync(
        TeacherId teacherId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.GetFileMetadataAsync(
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
        return await _blobRepository.GetFileUrlAsync(
            fileName,
            subfolders: GetSubfolders(teacherId),
            expiry: expiry,
            cancellationToken: cancellationToken);
    }

    private static string[] GetSubfolders(TeacherId teacherId) =>
        [teacherId.Value.ToString(), PhotosSubfolder];
}
