using Ardalis.Result;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.Teachers.Commands;

public static class UpdateTeacherDetails
{
    public record TeacherPhoto(byte[] Content, string ContentType, string FileName);
    public sealed record Command(TeacherForUpdate teacher, TeacherPhoto? Photo) : ICommand<Result<TeacherId>>;

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
            var teacher = await _readRepository.GetByIdAsync(command.teacher.Id, cancellationToken);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher with an id of {TeacherId} was not found.", command.teacher.Id);
                return Result.NotFound($"Teacher with an id of {command.teacher.Id} was not found.");
            }

            teacher
                .UpdateFirstName(command.teacher.FirstName)
                .UpdateMiddleName(command.teacher.MiddleName)
                .UpdateLastName(command.teacher.LastName)
                .UpdateTeacherIdentifier(command.teacher.TeacherIdentifier)
                .UpdateEmail(command.teacher.Email)
                .UpdatePhoneNumber(command.teacher.PhoneNumber)
                .UpdateDepartmentId(command.teacher.DepartmentId)
                .UpdateAcademicTitle(command.teacher.AcademicTitle)
                .UpdateQualification(command.teacher.Qualification)
                .UpdateSpecialization(command.teacher.Specialization)
                .UpdateOfficeLocation(command.teacher.OfficeLocation)
                .UpdateOfficeHours(command.teacher.OfficeHours)
                .UpdateBiography(command.teacher.Biography);

            var result = await _teacherRepository.Update(teacher, cancellationToken);

            if (result.IsSuccess && command.Photo != null)
            {
                using var stream = new MemoryStream(command.Photo.Content);
                var newFileName = await _teacherPhotoStorageService.UploadPhotoAsync(
                    teacher.TeacherIdentifier, command.Photo.FileName, stream, command.Photo.ContentType, cancellationToken: cancellationToken);

                teacher.UpdatePhoto(newFileName, command.Photo.ContentType);

                await _teacherRepository.Update(teacher, cancellationToken);
            }

            return result;
        }
    }
}
