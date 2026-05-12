using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Models;

public class ClassSectionForCreation
{
    public string Name { get; set; } = null!;
    public YearLevel YearLevel { get; set; }
    public CourseId CourseId { get; set; }
    public AcademicTermId AcademicTermId { get; set; }
    public TeacherId AdviserId { get; set; }
    public SectionCode SectionCode { get; set; }
}
