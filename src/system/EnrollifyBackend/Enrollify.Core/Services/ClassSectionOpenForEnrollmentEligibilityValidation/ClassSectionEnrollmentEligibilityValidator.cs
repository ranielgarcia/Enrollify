using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.Services.ClassScheduleValidation;

namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;

public static class ClassSectionEnrollmentEligibilityValidator
{
  public static ClassSectionOpenForEnrollmentEligibilityValidationContext Validate(
    ClassSection classSection,
    IReadOnlyCollection<ClassSectionSubjectOffering> offerings)
  {
    var context = new ClassSectionOpenForEnrollmentEligibilityValidationContext(classSection, offerings);

    if (classSection.AdviserId is null)
      context.AddClassSectionValidationMessage(DomainValidationErrorSeverityEnum.Error,
        "CLASS_SECTION_ADVISER_REQUIRED",
        "Class section must have an adviser assigned.");

    if (offerings.Count == 0)
    {
      context.AddClassSectionValidationMessage(DomainValidationErrorSeverityEnum.Error,
        "CLASS_SECTION_SUBJECT_OFFERINGS_MISSING",
        "Class section must have subject offerings.");
      return context;
    }

    foreach (ClassSectionSubjectOffering offering in offerings)
    {
      if (offering.SubjectUnitsOverride is null)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Info, offering.Id,
          "SUBJECT_OFFERING_UNITS_CAN_BE_OVERRIDEN",
          "Subject units can be overridden for this offering.");

      if (offering.TeacherId is null)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Error, offering.Id,
          "SUBJECT_OFFERING_TEACHER_REQUIRED",
          "Subject offering must have a teacher assigned.");

      if (offering.RoomId is null)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Error, offering.Id,
          "SUBJECT_OFFERING_ROOM_REQUIRED",
          "Subject offering must have a room assigned.");

      if (offering.DaysPerWeek == 1)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Warning, offering.Id,
          "SUBJECT_OFFERING_DAYS_PER_WEEK_DEFAULT_VALUE",
          "Days per week is set to the default value of 1.");

      if (offering.HoursPerDay == 1)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Warning, offering.Id,
          "SUBJECT_OFFERING_HOURS_PER_DAY_DEFAULT_VALUE",
          "Hours per day is set to the default value of 1.");

      if (offering.MaxNumberOfStudents < 1)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Warning, offering.Id,
          "SUBJECT_OFFERING_MAX_NUMBER_OF_STUDENTS_DEFAULT_VALUE",
          "Max number of students is set to the default value of 0, which means no limit.");

      if (!ClassScheduleValidationService.AreSchedulesSufficient(offering.ClassSchedules, offering.DaysPerWeek))
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Error, offering.Id,
          "SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES",
          "Subject offering must have class schedules equal to expected days per week.");

      if (ClassScheduleValidationService.ValidateAllSchedules(offering.ClassSchedules, offering.DaysPerWeek,
            offering.HoursPerDay) is ClassScheduleValidationResult scheduleValidationResult &&
          !scheduleValidationResult.IsValid)
        context.AddOfferingValidationMessage(DomainValidationErrorSeverityEnum.Error, offering.Id,
          "SUBJECT_OFFERING_INVALID_CLASS_SCHEDULES",
          $"Subject offering has invalid class schedules: {scheduleValidationResult.ErrorMessage}");
    }

    return context;
  }
}
