using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class
  GetClassSectionEnrollmentEligibilityValidationErrorsSpec : Specification<
  ClassSectionEnrollmentEligibilityValidationMessage>
{
  public GetClassSectionEnrollmentEligibilityValidationErrorsSpec(List<ClassSectionId> classSectionIds)
  {
    Query
      .Where(m => classSectionIds.Contains(m.ClassSectionId) && m.Severity == DomainValidationErrorSeverityEnum.Error);
  }
}
