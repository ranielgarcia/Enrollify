namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation.Rules;

public class ClassSectionSubjectOfferingIsRequiredRule : IClassSectionOpenForEnrollmentEligibilityValidationRule
{
  public int Order => 1;

  public void Validate(ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    if (context.ClassSectionSubjectOfferings.Count == 0)
      context.Invalidate("CLASS_SECTION_SUBJECT_OFFERINGS_MISSING", "Class section must have subject offerings.");
  }
}
