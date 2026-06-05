using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.ClassSectionAggregate;

/// <summary>
/// A projection entity that stores the result of running
/// <see cref="Services.ClassSectionOpenForEnrollmentEligibilityValidation.ClassSectionEnrollmentEligibilityValidator"/>
/// for a class section. Records are fully replaced (delete + insert) on every recomputation.
/// </summary>
public class ClassSectionEnrollmentEligibilityValidationMessage
  : EntityBase<ClassSectionEnrollmentEligibilityValidationMessage,
    ClassSectionEnrollmentEligibilityValidationMessageId>, IAggregateRoot
{
  private ClassSectionEnrollmentEligibilityValidationMessage()
  {
  }

  public ClassSectionEnrollmentEligibilityValidationMessage(
    DomainValidationErrorSeverityEnum severity,
    ClassSectionId classSectionId,
    ClassSectionSubjectOfferingId? offeringId,
    string code,
    string message)
  {
    Severity = severity;
    ClassSectionId = Guard.Against.Null(classSectionId, nameof(classSectionId));
    OfferingId = offeringId;
    Code = Guard.Against.NullOrEmpty(code, nameof(code));
    Message = Guard.Against.NullOrEmpty(message, nameof(message));
    ComputedAt = DateTimeOffset.UtcNow;
  }

  public DomainValidationErrorSeverityEnum Severity { get; private set; }

  public ClassSectionId ClassSectionId { get; private set; }

  public ClassSectionSubjectOfferingId? OfferingId { get; private set; }

  /// <summary>Validation error code, e.g. "CLASS_SECTION_ADVISER_REQUIRED".</summary>
  public string Code { get; private set; } = null!;

  /// <summary>Human-readable error message.</summary>
  public string Message { get; private set; } = null!;

  public DateTimeOffset ComputedAt { get; private set; }
}
