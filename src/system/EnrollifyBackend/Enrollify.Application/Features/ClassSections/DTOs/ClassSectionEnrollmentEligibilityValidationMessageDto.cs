using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.ClassSections.DTOs;

public class ClassSectionEnrollmentEligibilityValidationMessageBaseDto
{
  public DomainValidationErrorSeverityEnum Severity { get; set; } = null!;

  public string Code { get; set; } = null!;

  public string Message { get; set; } = null!;
  public DateTimeOffset ComputedAt { get; set; }
}

public class ClassSectionSubjectOfferingValidationMessageDto : ClassSectionEnrollmentEligibilityValidationMessageBaseDto
{
  public ClassSectionSubjectOfferingId OfferingId { get; set; }

  public static ClassSectionSubjectOfferingValidationMessageDto FromEntity(
    ClassSectionEnrollmentEligibilityValidationMessage entity)
  {
    return new ClassSectionSubjectOfferingValidationMessageDto
    {
      Severity = entity.Severity,
      OfferingId = entity.OfferingId!.Value,
      Code = entity.Code,
      Message = entity.Message,
      ComputedAt = entity.ComputedAt
    };
  }
}

public class
  ClassSectionEnrollmentEligibilityValidationMessageDto : ClassSectionEnrollmentEligibilityValidationMessageBaseDto
{
  public static ClassSectionEnrollmentEligibilityValidationMessageDto FromEntity(
    ClassSectionEnrollmentEligibilityValidationMessage entity)
  {
    return new ClassSectionEnrollmentEligibilityValidationMessageDto
    {
      Severity = entity.Severity,
      Code = entity.Code,
      Message = entity.Message,
      ComputedAt = entity.ComputedAt
    };
  }
}

public class ClassSectionEnrollmentEligibilityValidationMessagesDto
{
  public ClassSectionId ClassSectionId { get; set; }

  public List<ClassSectionEnrollmentEligibilityValidationMessageDto>
    ClassSectionValidationMessages { get; set; } = [];

  public Dictionary<ClassSectionSubjectOfferingId, List<ClassSectionSubjectOfferingValidationMessageDto>>
    OfferingsValidationMessages { get; set; } = [];

  public static ClassSectionEnrollmentEligibilityValidationMessagesDto FromEntities(
    ClassSectionId classSectionId,
    List<ClassSectionEnrollmentEligibilityValidationMessageDto> classSectionValidationMessages,
    List<ClassSectionSubjectOfferingValidationMessageDto>
      offeringsValidationMessages)
  {
    return new ClassSectionEnrollmentEligibilityValidationMessagesDto
    {
      ClassSectionId = classSectionId,
      ClassSectionValidationMessages = classSectionValidationMessages,
      OfferingsValidationMessages = offeringsValidationMessages.GroupBy(x => x.OfferingId)
        .ToDictionary(g => g.Key, g => g.ToList())
    };
  }
}
