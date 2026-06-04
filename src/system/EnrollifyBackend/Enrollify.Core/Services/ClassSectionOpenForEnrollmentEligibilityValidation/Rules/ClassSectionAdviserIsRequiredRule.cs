namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation.Rules;

public class ClassSectionAdviserIsRequiredRule : IClassSectionOpenForEnrollmentEligibilityValidationRule
{
  public int Order => 1;

  public void Validate(ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    if (context.ClassSection.AdviserId is null)
      context.Invalidate("CLASS_SECTION_ADVISER_REQUIRED", "Class section must have an adviser assigned.");
  }
}
