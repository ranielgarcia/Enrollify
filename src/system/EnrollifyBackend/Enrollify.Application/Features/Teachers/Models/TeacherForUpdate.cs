using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Application.Features.Teachers.Models;

public class TeacherForUpdate
{
    public TeacherId Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;

    public TeacherIdentifier TeacherIdentifier { get; set; }
    public TeacherEmail Email { get; set; }
    public TeacherPhoneNumber PhoneNumber { get; set; }
    public DepartmentId DepartmentId { get; set; }

    public string? AcademicTitle { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public string? OfficeLocation { get; set; }
    public string? OfficeHours { get; set; }
    public string? Biography { get; set; }

    public SubjectCode[] Subjects { get; set; } = [];
}
