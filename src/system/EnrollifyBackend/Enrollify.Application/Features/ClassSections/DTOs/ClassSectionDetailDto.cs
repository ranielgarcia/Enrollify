using Enrollify.Application.Features.ClassSectionSubjectOfferings.DTOs;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.ClassSections.DTOs;

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
  public ClassSectionEnrollmentEligibilityValidationMessagesDto ValidationMessages { get; init; } = new();
  public int UnresolvedErrorsCount { get; set; }

  public bool IsEligibleForOpenEnrollment { get; set; }

  public static ClassSectionDetailDto FromEntities(
    ClassSection section,
    IEnumerable<ClassSectionSubjectOffering> offerings,
    List<ClassSectionEnrollmentEligibilityValidationMessage> allValidationMessages)
  {
    var subjectOfferingsValidationMessages = allValidationMessages.Where(x => x.OfferingId != null)
      .Select(ClassSectionSubjectOfferingValidationMessageDto.FromEntity).ToList();
    var sectionValidationMessages = allValidationMessages.Where(x => x.OfferingId == null)
      .Select(ClassSectionEnrollmentEligibilityValidationMessageDto.FromEntity)
      .ToList();

    var validationMessages = ClassSectionEnrollmentEligibilityValidationMessagesDto.FromEntities(section.Id,
      sectionValidationMessages, subjectOfferingsValidationMessages);

    return new ClassSectionDetailDto
    {
      Id = section.Id,
      Name = section.Name,
      SectionCode = (char)section.SectionCode,
      IntendedYearLevel = (int)section.IntendedYearLevel,
      FullName = section.FullName,
      ValidationMessages = validationMessages,
      UnresolvedErrorsCount = allValidationMessages.Count(x => x.Severity == DomainValidationErrorSeverityEnum.Error),
      IsEligibleForOpenEnrollment = allValidationMessages.All(m =>
        m.Severity != DomainValidationErrorSeverityEnum.Error),

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

      Offerings = offerings
        .Select(ClassSectionSubjectOfferingDto.FromEntity)
        .ToList()
        .AsReadOnly(),

      CreatedAt = section.CreatedAt,
      CreatedBy = BaseUserDto.FromUser(section.CreatedByUser),
      UpdatedAt = section.UpdatedAt,
      UpdatedBy = section.UpdatedByUser is not null ? BaseUserDto.FromUser(section.UpdatedByUser) : null,
      IsActive = section.IsActive
    };
  }
}
