using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Core.Aggregates.TeacherAggregate.Models;

public class TeacherForCreation
{
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public TeacherIdentifier TeacherIdentifier { get; set; }
    public TeacherEmail Email { get; set; }
    public TeacherPhoneNumber PhoneNumber { get; set; }
    public DepartmentId DepartmentId { get; set; }

    public string AcademicTitle { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string OfficeLocation { get; set; } = null!;
    public string OfficeHours { get; set; } = null!;
    public string Biography { get; set; } = null!;
}
