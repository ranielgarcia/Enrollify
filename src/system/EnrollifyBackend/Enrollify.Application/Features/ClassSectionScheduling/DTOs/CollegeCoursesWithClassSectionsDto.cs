using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public class CollegeCoursesWithClassSectionsDto
{
  public CollegeId Id { get; set; }
  public string Code { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public List<CourseWithClassSectionsDto> CourseWithClassSections { get; set; } = [];

  public static CollegeCoursesWithClassSectionsDto FromEntity(College entity, List<CourseWithClassSectionsDto> courses)
  {
    return new CollegeCoursesWithClassSectionsDto
    {
      Id = entity.Id,
      Code = entity.Code.Value,
      Name = entity.Name,
      Description = entity.Description,
      CourseWithClassSections = courses
    };
  }
}

public class CourseWithClassSectionsDto
{
  public CourseId Id { get; set; }
  public CourseCode Code { get; private set; }
  public string Name { get; private set; } = null!;

  public List<ClassSectionDto> ClassSections { get; set; } = [];

  public static CourseWithClassSectionsDto FromEntity(Course entity, List<ClassSectionDto> classSections)
  {
    return new CourseWithClassSectionsDto
    {
      Id = entity.Id,
      Code = entity.Code,
      Name = entity.Name,
      ClassSections = classSections
    };
  }
}
