using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.DTOs;

public class ClassSectionDetailDto : BaseDto
{
  public ClassSectionId Id { get; set; }
  public string Name { get; set; } = null!;
  public char SectionCode { get; set; }
  public int IntendedYearLevel { get; set; }
  public string FullName { get; set; } = null!;

  public ClassSectionCourseDto Course { get; set; } = null!;
  public ClassSectionCurriculumDto Curriculum { get; set; } = null!;
  public ClassSectionAcademicTermDto AcademicTerm { get; set; } = null!;
  public ClassSectionCohortYearDto CohortAcademicYear { get; set; } = null!;
  public ClassSectionAdviserDto? Adviser { get; set; }
  public ClassSectionStatusDto Status { get; set; } = null!;

  public IReadOnlyList<ClassSectionSubjectOfferingDto> Offerings { get; set; } = [];
  public IReadOnlyList<ClassSectionValidationIssueDto> ValidationIssues { get; set; } = [];

  public int TotalUnresolveValidationIssues { get; set; }

  public static ClassSectionDetailDto FromEntities(
    ClassSection section,
    List<ClassSectionSubjectOffering> offerings,
    List<ClassSectionValidationIssue> validationIssues)
  {
    IEnumerable<ClassSectionValidationIssue> classSectionLevelValidationIssues =
      validationIssues.Where(x => x.OfferingId == null);
    var offeringValidationIssuesLookup = validationIssues
      .Where(x => x.OfferingId != null)
      .GroupBy(x => x.OfferingId!.Value)
      .ToDictionary(g => g.Key, g => g.ToList());

    // Convert offerings to DTOs
    var offeringDtos = offerings.Select(o =>
    {
      List<ClassSectionValidationIssue> offeringValidationIssues =
        offeringValidationIssuesLookup.TryGetValue(o.Id, out List<ClassSectionValidationIssue>? issues)
          ? issues
          : new List<ClassSectionValidationIssue>();
      return ClassSectionSubjectOfferingDto.FromEntity(o, offeringValidationIssues);
    }).ToList();

    return new ClassSectionDetailDto
    {
      Id = section.Id,
      Name = section.Name,
      SectionCode = (char)section.SectionCode,
      IntendedYearLevel = (int)section.IntendedYearLevel,
      FullName = section.FullName,
      ValidationIssues = classSectionLevelValidationIssues.Select(ClassSectionValidationIssueDto.FromEntity).ToList(),
      TotalUnresolveValidationIssues = validationIssues.Count(),

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

      Offerings = offeringDtos.AsReadOnly(),

      CreatedAt = section.CreatedAt,
      CreatedBy = BaseUserDto.FromUser(section.CreatedByUser),
      UpdatedAt = section.UpdatedAt,
      UpdatedBy = section.UpdatedByUser is not null ? BaseUserDto.FromUser(section.UpdatedByUser) : null,
      IsActive = section.IsActive
    };
  }
}
