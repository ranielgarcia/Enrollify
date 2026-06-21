using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public class ClassSectionDto : BaseDto
{
  public ClassSectionId Id { get; set; }
  public string Name { get; set; } = null!;
  public string FullName { get; set; } = null!;
  public char SectionCode { get; set; }
  public int IntendedYearLevel { get; set; }

  public ClassSectionCourseDto Course { get; set; } = null!;
  public ClassSectionCurriculumDto Curriculum { get; set; } = null!;
  public ClassSectionAcademicTermDto AcademicTerm { get; set; } = null!;
  public ClassSectionCohortYearDto CohortAcademicYear { get; set; } = null!;
  public ClassSectionAdviserDto? Adviser { get; set; }
  public ClassSectionStatusDto Status { get; set; } = null!;

  public static ClassSectionDto FromEntity(ClassSection section)
  {
    return new ClassSectionDto
    {
      Id = section.Id,
      Name = section.Name,
      FullName = section.FullName,
      SectionCode = (char)section.SectionCode,
      IntendedYearLevel = (int)section.IntendedYearLevel,

      Course = section.Course is not null
        ? new ClassSectionCourseDto
        {
          Id = section.Course.Id,
          Code = (string)section.Course.Code,
          Name = section.Course.Name
        }
        : null!,

      Curriculum = section.Curriculum is not null
        ? new ClassSectionCurriculumDto
        {
          Id = section.Curriculum.Id,
          Version = section.Curriculum.Version
        }
        : null!,

      AcademicTerm = section.AcademicTerm is not null
        ? new ClassSectionAcademicTermDto
        {
          Id = section.AcademicTerm.Id,
          TermNumber = section.AcademicTerm.TermNumber.Value,
          TermName = section.AcademicTerm.TermName
        }
        : null!,

      CohortAcademicYear = section.CohortAcademicYear is not null
        ? new ClassSectionCohortYearDto
        {
          Id = section.CohortAcademicYear.Id,
          AcademicYearTitle = section.CohortAcademicYear.AcademicYearTitle
        }
        : null!,

      Adviser = section.Adviser is not null
        ? new ClassSectionAdviserDto
        {
          Id = section.Adviser.Id,
          FirstName = section.Adviser.FirstName,
          LastName = section.Adviser.LastName,
          Email = (string)section.Adviser.Email
        }
        : null,

      Status = new ClassSectionStatusDto
      {
        Name = section.StatusId.Name,
        Value = section.StatusId.Value,
        Description = section.StatusId.Description
      },

      CreatedAt = section.CreatedAt,
      CreatedBy = BaseUserDto.FromUser(section.CreatedByUser),
      UpdatedAt = section.UpdatedAt,
      UpdatedBy = section.UpdatedByUser is not null ? BaseUserDto.FromUser(section.UpdatedByUser) : null,
      IsActive = section.IsActive
    };
  }
}

public class ClassSectionCourseDto
{
  public CourseId Id { get; set; }
  public string Code { get; set; } = null!;
  public string Name { get; set; } = null!;
}

public class ClassSectionCurriculumDto
{
  public CurriculumId Id { get; set; }
  public string Version { get; set; } = null!;
}

public class ClassSectionAcademicTermDto
{
  public AcademicTermId Id { get; set; }
  public int TermNumber { get; set; }
  public string TermName { get; set; } = null!;
}

public class ClassSectionCohortYearDto
{
  public AcademicYearId Id { get; set; }
  public string AcademicYearTitle { get; set; } = null!;
}

public class ClassSectionAdviserDto
{
  public TeacherId Id { get; set; }
  public string FirstName { get; set; } = null!;
  public string LastName { get; set; } = null!;
  public string Email { get; set; } = null!;
}

public class ClassSectionStatusDto
{
  public string Name { get; set; } = null!;
  public int Value { get; set; }
  public string Description { get; set; } = null!;
}
