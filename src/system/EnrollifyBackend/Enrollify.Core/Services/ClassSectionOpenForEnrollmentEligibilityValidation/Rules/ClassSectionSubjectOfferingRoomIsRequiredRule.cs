using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation.Rules;

public class ClassSectionSubjectOfferingRoomIsRequiredRule : IClassSectionOpenForEnrollmentEligibilityValidationRule
{
  public int Order => 3;

  public void Validate(ClassSectionOpenForEnrollmentEligibilityValidationContext context)
  {
    if (!context.ClassSectionSubjectOfferings.Any()) return;
    foreach (ClassSectionSubjectOffering offering in context.ClassSectionSubjectOfferings)
      if (offering.RoomId is null)
        context.InvalidateOffering(offering.Id, "SUBJECT_OFFERING_ROOM_REQUIRED",
          "Subject offering must have a room assigned.");
  }
}
