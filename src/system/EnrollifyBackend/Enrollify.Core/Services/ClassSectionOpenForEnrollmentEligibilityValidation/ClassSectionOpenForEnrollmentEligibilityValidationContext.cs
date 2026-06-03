using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
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

  public List<DomainValidationMessage> ValidationErrors { get; private set; } = [];
  public List<DomainValidationMessage> InformationalMessages { get; private set; } = [];
  public List<DomainValidationMessage> SoftRulesMessages { get; private set; } = [];

  public Dictionary<ClassSectionSubjectOfferingId, List<DomainValidationMessage>>
    OfferingValidationErrors { get; set; } = new();

  public bool IsEligible => ValidationErrors.Count == 0;

  public void Invalidate(string errorCode, string message)
  {
    ValidationErrors.Add(new DomainValidationMessage(errorCode, message));
  }

  public void InvalidateOffering(ClassSectionSubjectOfferingId offeringId, string errorCode, string message)
  {
    if (!OfferingValidationErrors.ContainsKey(offeringId))
      OfferingValidationErrors.Add(offeringId, new List<DomainValidationMessage> { new(errorCode, message) });
    else
      OfferingValidationErrors[offeringId].Add(new DomainValidationMessage(errorCode, message));
  }

  public void AddInformationalMessage(string errorCode, string message)
  {
    InformationalMessages.Add(new DomainValidationMessage(errorCode, message));
  }

  public void AddSoftRuleMessage(string errorCode, string message)
  {
    SoftRulesMessages.Add(new DomainValidationMessage(errorCode, message));
  }
}
