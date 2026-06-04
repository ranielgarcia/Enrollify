using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation.Rules;

public class
  ClassSectionSubjectOfferingDaysPerWeekWithDefaultValueInformationalRule :
  IClassSectionOpenForEnrollmentEligibilityValidationRule
{
  public int Order => 3;

  public void Validate(ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    if (!context.ClassSectionSubjectOfferings.Any()) return;
    foreach (ClassSectionSubjectOffering offering in context.ClassSectionSubjectOfferings)
      if (offering.DaysPerWeek == 1)
        context.AddInformationalMessage("SUBJECT_OFFERING_DAYS_PER_WEEK_DEFAULT_VALUE",
          "Days per week is set to the default value of 1.");
  }
}
