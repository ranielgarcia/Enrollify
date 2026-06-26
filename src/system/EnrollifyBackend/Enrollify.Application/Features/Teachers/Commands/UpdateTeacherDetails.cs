using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Features.Teachers.Storage;

namespace Enrollify.Application.Features.Teachers.Commands;

public static class UpdateTeacherDetails
{
    public record TeacherPhoto(byte[] Content, string ContentType, string FileName);
    public sealed record Command(TeacherForUpdate teacher, TeacherPhoto? Photo) : IRequest<Result<TeacherId>>;

    public sealed class Handler : IRequestHandler<Command, Result<TeacherId>>
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

        public async Task<Result<TeacherId>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (command.teacher.Subjects == null || !command.teacher.Subjects.Any())
            {
                return Result.Invalid(new ValidationError("At least one subject is required"));
            }

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

            var allSubjects = await _subjectReadRepository.ListAsync(new ListMinimumSubjectsByCodesSpec(command.teacher.Subjects.ToList()), cancellationToken);
            var missingSubjectCodes = command.teacher.Subjects.Except(allSubjects.Select(s => s.Code)).ToList();
            if (missingSubjectCodes.Count > 0)
            {
                return Result.Invalid(new ValidationError($"Subjects with codes {string.Join(", ", missingSubjectCodes)} not found"));
            }

            var allSubjectIds = allSubjects.Select(s => s.Id).ToHashSet();
            var existingSubjectIds = teacher.GetActiveSubjects().Select(s => s.SubjectId).ToHashSet();

            // Remove subjects that are no longer associated with the teacher
            var subjectsToRemove = existingSubjectIds.Where(sId => !allSubjectIds.Contains(sId)).ToList();
            foreach(var subjectId in subjectsToRemove)
            {
                teacher.RemoveSubject(subjectId);
            }

            // Add new subjects
            var newSubjectsToAdd = allSubjects.Where(s => !existingSubjectIds.Contains(s.Id)).ToList();
            foreach(var subject in newSubjectsToAdd)
            {
                teacher.AddSubject(subject.Id);
            }


            if (command.Photo != null)
            {
                using var stream = new MemoryStream(command.Photo.Content);
                var newFileName = await _teacherPhotoStorageService.UploadPhotoAsync(
                    teacher.TeacherIdentifier, command.Photo.FileName, stream, command.Photo.ContentType, cancellationToken: cancellationToken);

                teacher.UpdatePhoto(newFileName, command.Photo.ContentType);
            }

            var result = await _teacherRepository.Update(teacher, cancellationToken);

            return result;
        }
    }
}
