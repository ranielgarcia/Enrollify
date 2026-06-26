using Enrollify.Application.Storage;
using Enrollify.Application.Storage.Blob;
using Enrollify.Core.ValueObjects.Storage;

namespace Enrollify.Application.Features.Teachers.Storage;

public interface ITeacherPhotoStorageService
{
    Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);
    Task<bool> DeletePhotoAsync(TeacherIdentifier teacherIdentifier, FileName fileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredFile>> GetAllPhotosAsync(TeacherIdentifier teacherIdentifier, CancellationToken cancellationToken = default);
    Task<StoredFile?> GetPhotoAsync(TeacherIdentifier teacherIdentifier, FileName fileName, CancellationToken cancellationToken = default);
    Task<FileMetadata?> GetPhotoMetadataAsync(TeacherIdentifier teacherIdentifier, FileName fileName, CancellationToken cancellationToken = default);
    Task<string?> GetPhotoUrlAsync(TeacherIdentifier teacherIdentifier, FileName fileName, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileMetadata>> ListPhotosAsync(TeacherIdentifier teacherIdentifier, CancellationToken cancellationToken = default);
    Task<bool> PhotoExistsAsync(TeacherIdentifier teacherIdentifier, FileName fileName, CancellationToken cancellationToken = default);
    Task<FileName> UploadPhotoAsync(TeacherIdentifier teacherIdentifier, string originalFileName, Stream content, string? contentType = null, bool overwrite = true, CancellationToken cancellationToken = default);
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

    public async Task<FileName> UploadPhotoAsync(
        TeacherIdentifier teacherIdentifier,
        string originalFileName,
        Stream content,
        string? contentType = null,
        bool overwrite = true,
        CancellationToken cancellationToken = default)
    {
        var newFileName = GetFileName(originalFileName);

        await _blobRepository.UploadFileAsync(
            newFileName,
            content,
            subfolders: GetSubfolders(teacherIdentifier),
            contentType: contentType,
            overwrite: overwrite,
            cancellationToken: cancellationToken);

        return newFileName;
    }

    public async Task<StoredFile?> GetPhotoAsync(
        TeacherIdentifier teacherIdentifier,
        FileName fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.GetFileAsync(
            fileName,
            subfolders: GetSubfolders(teacherIdentifier),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetAllPhotosAsync(
        TeacherIdentifier teacherIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.GetFilesAsync(
            subfolders: GetSubfolders(teacherIdentifier),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> DeletePhotoAsync(
        TeacherIdentifier teacherIdentifier,
        FileName fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.DeleteFileAsync(
            fileName,
            subfolders: GetSubfolders(teacherIdentifier),
            cancellationToken: cancellationToken);
    }

    public async Task<bool> PhotoExistsAsync(
        TeacherIdentifier teacherIdentifier,
        FileName fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.FileExistsAsync(
            fileName,
            subfolders: GetSubfolders(teacherIdentifier),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<FileMetadata>> ListPhotosAsync(
        TeacherIdentifier teacherIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.ListFilesAsync(
            subfolders: GetSubfolders(teacherIdentifier),
            cancellationToken: cancellationToken);
    }

    public async Task<FileMetadata?> GetPhotoMetadataAsync(
        TeacherIdentifier teacherIdentifier,
        FileName fileName,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.GetFileMetadataAsync(
            fileName,
            subfolders: GetSubfolders(teacherIdentifier),
            cancellationToken: cancellationToken);
    }

    public async Task<string?> GetPhotoUrlAsync(
        TeacherIdentifier teacherIdentifier,
        FileName fileName,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        return await _blobRepository.GetFileUrlAsync(
            fileName,
            subfolders: GetSubfolders(teacherIdentifier),
            expiry: expiry,
            cancellationToken: cancellationToken);
    }

    private static FileName GetFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var guid = Guid.NewGuid().ToString();
        return FileName.From($"{guid}_profile{extension}");
    }

    private static string[] GetSubfolders(TeacherIdentifier teacherIdentifier) =>
        new[] { teacherIdentifier.Value.ToString(), PhotosSubfolder };
}
