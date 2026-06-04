using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.Models;

namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;

public sealed class ClassSectionOpenForEnrollmentEligibilityValidationContext
{
  public ClassSection ClassSection { get; init; }
  public IReadOnlyCollection<ClassSectionSubjectOffering> ClassSectionSubjectOfferings { get; init; }

  public ClassSectionOpenForEnrollmentEligibilityValidationContext(ClassSection classSection,
    IReadOnlyCollection<ClassSectionSubjectOffering> classSectionSubjectOfferings)
  {
    Guard.Against.Null(classSection, nameof(classSection));
    // Guard.Against.NullOrEmpty(classSectionSubjectOfferings, nameof(classSectionSubjectOfferings));

    if (classSectionSubjectOfferings.Any(o => o.ClassSectionId != classSection.Id))
      throw new InvalidSubjectOfferingForClassSectionException(
        "All subject offerings must belong to the provided class section.");

    ClassSection = classSection;
    ClassSectionSubjectOfferings = classSectionSubjectOfferings.ToList();
  }

  public List<DomainValidationMessage> ClassSectionValidationMessages { get; private set; } = [];

  public Dictionary<ClassSectionSubjectOfferingId, List<DomainValidationMessage>>
    OfferingValidationMessages { get; set; } = new();

  public bool IsEligible => ClassSectionValidationMessages.Count == 0 &&
                            OfferingValidationMessages.All(kv => kv.Value.Count == 0);

  public void AddClassSectionValidationMessage(DomainValidationErrorSeverityEnum severity, string errorCode,
    string message)

  {
    ClassSectionValidationMessages.Add(new DomainValidationMessage(severity, errorCode,
      message));
  }

  public void AddOfferingValidationMessage(DomainValidationErrorSeverityEnum severity,
    ClassSectionSubjectOfferingId offeringId,
    string errorCode, string message)
  {
    if (!OfferingValidationMessages.ContainsKey(offeringId))
      OfferingValidationMessages.Add(offeringId,
        new List<DomainValidationMessage> { new(severity, errorCode, message) });
    else
      OfferingValidationMessages[offeringId]
        .Add(new DomainValidationMessage(severity, errorCode, message));
  }
}
