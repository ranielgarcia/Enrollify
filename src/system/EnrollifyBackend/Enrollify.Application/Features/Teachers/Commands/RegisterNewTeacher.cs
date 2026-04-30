using Ardalis.Result;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.Teachers.Commands;

public static class RegisterNewTeacher
{
    public record TeacherPhoto(byte[] Content, string ContentType, string FileName);

    public sealed record Command (TeacherForRegistration teacher, TeacherPhoto? Photo) : ICommand<Result<TeacherId>>;

    public sealed class Handler : ICommandHandler<Command, Result<TeacherId>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherPhotoStorageService _teacherPhotoStorageService;
        private readonly IReadRepository<Teacher> _readRepository;
        private readonly IReadRepository<Subject> _subjectReadRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(ITeacherRepository teacherRepository,
            ITeacherPhotoStorageService teacherPhotoStorageService,
            IReadRepository<Teacher> readRepository,
            IReadRepository<Subject> subjectReadRepository,
            ILogger<Handler> logger)
        {
            _teacherRepository = teacherRepository;
            _teacherPhotoStorageService = teacherPhotoStorageService;
            _readRepository = readRepository;
            _subjectReadRepository = subjectReadRepository;
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


            var allSubjects = await _subjectReadRepository.ListAsync(new ListMinimumSubjectsByCodesSpec(command.teacher.Subjects.ToList()), cancellationToken);
            var missingSubjectCodes = command.teacher.Subjects.Except(allSubjects.Select(s => s.Code)).ToList();
            if (missingSubjectCodes.Count > 0)
            {
                return Result.Invalid(new ValidationError($"Subjects with codes {string.Join(", ", missingSubjectCodes)} not found"));
            }

            foreach(var subject in allSubjects)
            {
                teacher.AddSubject(subject.Id);
            }

            var result = await _teacherRepository.Create(teacher, cancellationToken);

            if (result.IsSuccess && command.Photo != null)
            {
                var teacherId = result.Value;
                await ProcessPhoto(teacherId, command.Photo, cancellationToken);
            }

            return result;
        }
        
        private async Task ProcessPhoto (TeacherId teacherId, TeacherPhoto photo, CancellationToken ct)
        {
            var newlyCreatedTeacher = await _readRepository.GetByIdAsync(teacherId, ct);

            if (newlyCreatedTeacher != null)
            {
                using var stream = new MemoryStream(photo.Content);
                var newFileName = await _teacherPhotoStorageService.UploadPhotoAsync(
                    newlyCreatedTeacher.TeacherIdentifier, photo.FileName, stream, photo.ContentType, cancellationToken: ct);

                newlyCreatedTeacher.UpdatePhoto(newFileName, photo.ContentType);

                await _teacherRepository.Update(newlyCreatedTeacher, ct);
            }
            else
            {
                _logger.LogError("Failed to retrieve the newly created teacher with ID {TeacherId} for photo upload.", teacherId);
            }
        }

    }
}
