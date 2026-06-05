using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class
  GetClassSectionEnrollmentEligibilityValidationMessagesSpec : Specification<
  ClassSectionEnrollmentEligibilityValidationMessage>
{
  public GetClassSectionEnrollmentEligibilityValidationMessagesSpec(List<ClassSectionId> classSectionIds)
  {
    Query.Where(m => classSectionIds.Contains(m.ClassSectionId));
  }

  public GetClassSectionEnrollmentEligibilityValidationMessagesSpec(ClassSectionId classSectionId)
  {
    Query.Where(m => m.ClassSectionId == classSectionId);
  }
}
