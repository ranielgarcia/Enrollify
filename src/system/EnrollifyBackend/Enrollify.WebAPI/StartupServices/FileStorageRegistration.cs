using Enrollify.Application.Features.Teachers.Storage;

namespace Enrollify.WebAPI.StartupServices;

public class FileStorageRegistration(ITeacherPhotoStorageService teacherPhotoStorageService) : IStartupService
{
    public async Task Initialize(CancellationToken cancellation)
    {
        await teacherPhotoStorageService.EnsureContainerExistsAsync(cancellation);
    }
}
