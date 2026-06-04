using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.ClassSections.DTOs;

public class ClassSectionEnrollmentEligibilityValidationMessageDto
{
  public DomainValidationErrorSeverityEnum Severity { get; set; } = null!;

  public ClassSectionId ClassSectionId { get; set; }

  public ClassSectionSubjectOfferingId? OfferingId { get; set; }

  /// <summary>Validation error code, e.g. "CLASS_SECTION_ADVISER_REQUIRED".</summary>
  public string Code { get; set; } = null!;

  /// <summary>Human-readable error message.</summary>
  public string Message { get; set; } = null!;

  public DateTimeOffset ComputedAt { get; set; }

  public static ClassSectionEnrollmentEligibilityValidationMessageDto FromEntity(
    ClassSectionEnrollmentEligibilityValidationMessage entity)
  {
    return new ClassSectionEnrollmentEligibilityValidationMessageDto
    {
      Severity = entity.Severity,
      ClassSectionId = entity.ClassSectionId,
      OfferingId = entity.OfferingId,
      Code = entity.Code,
      Message = entity.Message,
      ComputedAt = entity.ComputedAt
    };
  }
}
