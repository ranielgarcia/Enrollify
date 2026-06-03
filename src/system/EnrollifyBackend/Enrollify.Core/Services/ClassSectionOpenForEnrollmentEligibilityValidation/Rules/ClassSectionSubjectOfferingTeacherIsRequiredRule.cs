using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation.Rules;

public class ClassSectionSubjectOfferingTeacherIsRequiredRule : IClassSectionOpenForEnrollmentEligibilityValidationRule
{
  public int Order => 2;

  public void Validate(ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    if (!context.ClassSectionSubjectOfferings.Any()) return;
    foreach (ClassSectionSubjectOffering offering in context.ClassSectionSubjectOfferings)
      if (offering.TeacherId is null || offering.Teacher is null)
        context.InvalidateOffering(offering.Id, "SUBJECT_OFFERING_TEACHER_REQUIRED",
          "Subject offering must have a teacher assigned.");
  }
}
