namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;

public interface IClassSectionOpenForEnrollmentEligibilityValidationRule
{
  int Order { get; }
  void Validate(ClassSectionOpenForEnrollmentEligibilityValidationContext context);
}
