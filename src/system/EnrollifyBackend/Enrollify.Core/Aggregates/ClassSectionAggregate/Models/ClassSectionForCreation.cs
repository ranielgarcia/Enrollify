using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate.Models;

public class ClassSectionForCreation
{
    public required string Name { get; set; }
    public required YearLevel IntendedYearLevel { get; set; }
    public required CourseId CourseId { get; set; }
    public required CurriculumId CurriculumId { get; set; }
    public required AcademicTermId AcademicTermId { get; set; }
    public required AcademicYearId CohortAcademicYearId { get; set; }
    public TeacherId? AdviserId { get; set; }
    public required SectionCode SectionCode { get; set; }
    public required ClassSectionStatusEnum InitializeStatus { get; set; }
}
