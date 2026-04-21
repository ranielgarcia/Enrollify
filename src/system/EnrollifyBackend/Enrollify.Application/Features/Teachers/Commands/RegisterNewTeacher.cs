using Ardalis.Result;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate.Models;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Teachers.Commands;

public static class RegisterNewTeacher
{
    public record TeacherPhoto(byte[] Content, string ContentType, string FileName);

    public sealed record Command (TeacherForCreation teacherForCreation, TeacherPhoto? Photo) : ICommand<Result<TeacherId>>;

    public sealed class Handler : ICommandHandler<Command, Result<TeacherId>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherPhotoStorageService _teacherPhotoStorageService;

        public Handler(ITeacherRepository teacherRepository, ITeacherPhotoStorageService teacherPhotoStorageService)
        {
            _teacherRepository = teacherRepository;
            _teacherPhotoStorageService = teacherPhotoStorageService;
        }

        public async ValueTask<Result<TeacherId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var teacher = Teacher.Create(command.teacherForCreation);
            var result = await _teacherRepository.Create(teacher, cancellationToken);

            if (result.IsSuccess && command.Photo != null)
            {
                // upload the photo if any
                using var stream = new MemoryStream(command.Photo.Content);
                await _teacherPhotoStorageService.UploadPhotoAsync(
                    teacher.TeacherIdentifier, command.Photo.FileName, stream, command.Photo.ContentType, cancellationToken: cancellationToken);
            }

            return result;
        }


    }
}
