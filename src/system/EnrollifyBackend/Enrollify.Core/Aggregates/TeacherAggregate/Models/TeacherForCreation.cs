using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.Core.Aggregates.TeacherAggregate.Models;

public class TeacherForCreation
{
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public TeacherEmail Email { get; set; }

    public DepartmentId DepartmentId { get; set; }
}
