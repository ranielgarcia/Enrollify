namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;

public sealed class ClassSectionOpenForEnrollmentEligibilityValidationPipeline
{
  private readonly IEnumerable<IClassSectionOpenForEnrollmentEligibilityValidationRule> _rules;

  public ClassSectionOpenForEnrollmentEligibilityValidationPipeline(
    IEnumerable<IClassSectionOpenForEnrollmentEligibilityValidationRule> rules)
  {
    _rules = rules.OrderBy(r => r.Order);
  }

  public ClassSectionOpenForEnrollmentEligibilityValidationContext Validate(
    ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    foreach (IClassSectionOpenForEnrollmentEligibilityValidationRule rule in _rules) rule.Validate(context);

    return context;
  }
}
