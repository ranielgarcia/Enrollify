using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Constants;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.Services.ClassScheduleValidation;

namespace Enrollify.Core.Services.ClassSectionDataIntegrityValidation;

public class ClassSectionDataIntegrityValidator
{
  public List<ClassSectionDataIntegrityResult> Validate(ClassSection classSection,
    IReadOnlyCollection<ClassSectionSubjectOffering> offerings)
  {
    Guard.Against.Null(classSection, nameof(classSection));

    if (offerings.Any(o => o.ClassSectionId != classSection.Id))
      throw new InvalidSubjectOfferingForClassSectionException(
        "All subject offerings must belong to the provided class section.");

    List<ClassSectionDataIntegrityResult> validationResults = new();


    if (classSection.AdviserId is null)
      validationResults.Add(new ClassSectionDataIntegrityResult
      {
        ClassSectionId = classSection.Id,
        OfferingId = null,
        Type = ClassSectionValidationIssueTypeEnum.ADVISER_NOT_ASSIGNED,
        Message = "Class section is missing an adviser."
      });

    if (offerings.Count == 0)
      validationResults.Add(new ClassSectionDataIntegrityResult
      {
        ClassSectionId = classSection.Id,
        OfferingId = null,
        Type = ClassSectionValidationIssueTypeEnum.NO_OFFERINGS,
        Message = "Class section must have subject offerings."
      });

    foreach (ClassSectionSubjectOffering offering in offerings)
    {
      if (offering.TeacherId is null)
        validationResults.Add(new ClassSectionDataIntegrityResult
        {
          ClassSectionId = classSection.Id,
          OfferingId = offering.Id,
          Type = ClassSectionValidationIssueTypeEnum.TEACHER_NOT_ASSIGNED,
          Message = "Subject offering must have a teacher assigned."
        });

      if (offering.RoomId is null)
        validationResults.Add(new ClassSectionDataIntegrityResult
        {
          ClassSectionId = classSection.Id,
          OfferingId = offering.Id,
          Type = ClassSectionValidationIssueTypeEnum.ROOM_NOT_ASSIGNED,
          Message = "Subject offering must have a room assigned."
        });

      if (offering.DaysPerWeek == 1)
        validationResults.Add(new ClassSectionDataIntegrityResult
        {
          ClassSectionId = classSection.Id,
          OfferingId = offering.Id,
          Type = ClassSectionValidationIssueTypeEnum.DAYS_PER_WEEK_DEFAULT,
          Message = "Days per week is set to the default value of 1."
        });

      if (offering.HoursPerDay == 1)
        validationResults.Add(new ClassSectionDataIntegrityResult
        {
          ClassSectionId = classSection.Id,
          OfferingId = offering.Id,
          Type = ClassSectionValidationIssueTypeEnum.HOURS_PER_DAY_DEFAULT,
          Message = "Hours per day is set to the default value of 1."
        });

      if (offering.MaxNumberOfStudents == 1)
        validationResults.Add(new ClassSectionDataIntegrityResult
        {
          ClassSectionId = classSection.Id,
          OfferingId = offering.Id,
          Type = ClassSectionValidationIssueTypeEnum.MAX_STUDENTS_AT_DEFAULT,
          Message = "Max number of students is set to the default value of 0, which means no limit."
        });

      if (offering.ClassSchedules.Count == 0)
        validationResults.Add(new ClassSectionDataIntegrityResult
        {
          ClassSectionId = classSection.Id,
          OfferingId = offering.Id,
          Type = ClassSectionValidationIssueTypeEnum.NO_SCHEDULES,
          Message = "Subject offering must have a class schedules."
        });

      if (offering.ClassSchedules.Count > 0)
      {
        if (!ClassScheduleValidationService.AreSchedulesSufficient(offering.ClassSchedules, offering.DaysPerWeek))
          validationResults.Add(new ClassSectionDataIntegrityResult
          {
            ClassSectionId = classSection.Id,
            OfferingId = offering.Id,
            Type = ClassSectionValidationIssueTypeEnum.SCHEDULE_COUNT_MISMATCH,
            Message = "Subject offering must have class schedules equal to expected days per week."
          });

        if (ClassScheduleValidationService.ValidateAllSchedules(offering.ClassSchedules, offering.DaysPerWeek,
              offering.HoursPerDay) is ClassScheduleValidationResult scheduleValidationResult &&
            !scheduleValidationResult.IsValid)
          validationResults.Add(new ClassSectionDataIntegrityResult
          {
            ClassSectionId = classSection.Id,
            OfferingId = offering.Id,
            Type = ClassSectionValidationIssueTypeEnum.HOURS_MISMATCH,
            Message = $"Subject offering has invalid class schedules: {scheduleValidationResult.ErrorMessage}"
          });
      }
    }

    return validationResults;
  }
}
