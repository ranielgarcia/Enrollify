using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.Teachers.DTOs;

public class TeacherDto : BaseDto
{
    public TeacherId Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public TeacherIdentifier TeacherIdentifier { get; set; }
    public TeacherEmail Email { get; set; }
    public TeacherPhoneNumber PhoneNumber { get; set; }

    public DepartmentSummaryDto Department { get; set; } = null!;

    public string? AcademicTitle { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public string? OfficeLocation { get; set; }
    public string? OfficeHours { get; set; }
    public string? Biography { get; set; }

    public static TeacherDto FromEntity(Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            MiddleName = teacher.MiddleName,
            LastName = teacher.LastName,
            TeacherIdentifier = teacher.TeacherIdentifier,
            Email = teacher.Email,
            PhoneNumber = teacher.PhoneNumber,
            Department = teacher.Department != null ? DepartmentSummaryDto.FromEntity(teacher.Department) : new DepartmentSummaryDto(),
            AcademicTitle = teacher.AcademicTitle,
            Qualification = teacher.Qualification,
            Specialization = teacher.Specialization,
            OfficeLocation = teacher.OfficeLocation,
            OfficeHours = teacher.OfficeHours,
            Biography = teacher.Biography,
            CreatedAt = teacher.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(teacher.CreatedByUser),
            UpdatedAt = teacher.UpdatedAt,
            UpdatedBy = teacher.UpdatedByUser != null ? BaseUserDto.FromUser(teacher.UpdatedByUser) : null,
            IsActive = teacher.IsActive,
        };
    }

}
