using Ardalis.Result;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.Teachers.Commands;

public static class RegisterNewTeacher
{
    public record TeacherPhoto(byte[] Content, string ContentType, string FileName);

    public sealed record Command (TeacherForCreation teacher, TeacherPhoto? Photo) : ICommand<Result<TeacherId>>;

    public sealed class Handler : ICommandHandler<Command, Result<TeacherId>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherPhotoStorageService _teacherPhotoStorageService;
        private readonly IReadRepository<Teacher> _readRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(ITeacherRepository teacherRepository,
            ITeacherPhotoStorageService teacherPhotoStorageService,
            IReadRepository<Teacher> readRepository,
            ILogger<Handler> logger)
        {
            _teacherRepository = teacherRepository;
            _teacherPhotoStorageService = teacherPhotoStorageService;
            _readRepository = readRepository;
            _logger = logger;
        }

        public async ValueTask<Result<TeacherId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var teacher = new Teacher(
                    command.teacher.FirstName,
                    command.teacher.MiddleName,
                    command.teacher.LastName,
                    command.teacher.TeacherIdentifier,
                    command.teacher.Email,
                    command.teacher.PhoneNumber,
                    command.teacher.DepartmentId
                );

            teacher
                .UpdateAcademicTitle(command.teacher.AcademicTitle)
                .UpdateQualification(command.teacher.Qualification)
                .UpdateSpecialization(command.teacher.Specialization)
                .UpdateOfficeLocation(command.teacher.OfficeLocation)
                .UpdateOfficeHours(command.teacher.OfficeHours)
                .UpdateBiography(command.teacher.Biography);

            var result = await _teacherRepository.Create(teacher, cancellationToken);

            if (result.IsSuccess && command.Photo != null)
            {
                var teacherId = result.Value;
                var newlyCreatedTeacher = await _readRepository.GetByIdAsync(teacherId, cancellationToken);

                if (newlyCreatedTeacher != null)
                {
                    using var stream = new MemoryStream(command.Photo.Content);
                    var newFileName = await _teacherPhotoStorageService.UploadPhotoAsync(
                        newlyCreatedTeacher.TeacherIdentifier, command.Photo.FileName, stream, command.Photo.ContentType, cancellationToken: cancellationToken);

                    newlyCreatedTeacher.UpdatePhoto(newFileName, command.Photo.ContentType);

                    await _teacherRepository.Update(newlyCreatedTeacher, cancellationToken);
                }
                else
                {
                    _logger.LogError("Failed to retrieve the newly created teacher with ID {TeacherId} for photo upload.", teacherId);
                }
            }

            return result;
        }


    }
}
