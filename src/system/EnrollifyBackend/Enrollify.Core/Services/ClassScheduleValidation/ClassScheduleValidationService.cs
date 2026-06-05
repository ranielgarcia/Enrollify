using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;

namespace Enrollify.Core.Services.ClassScheduleValidation;

/// <summary>
/// Service for validating class schedule operations.
/// Contains reusable validation logic that can be used by both the aggregate root (throws exceptions)
/// and validators (returns validation results).
/// </summary>
public static class ClassScheduleValidationService
{
  private const decimal HourTolerance = 0.01m;

  /// <summary>
  /// Validates the addition of a new class schedule.
  /// </summary>
  /// <param name="existingSchedules">Collection of existing class schedules</param>
  /// <param name="newSchedule">The new schedule to be added</param>
  /// <param name="daysPerWeek">Expected number of days per week</param>
  /// <param name="hoursPerDay">Expected number of hours per day</param>
  /// <returns>Validation result with error message if validation fails</returns>
  public static ClassScheduleValidationResult ValidateAddClassSchedule(
    IReadOnlyCollection<ClassSchedule> existingSchedules,
    ClassSchedule newSchedule,
    int daysPerWeek,
    decimal hoursPerDay)
  {
    // Calculate hours for the new schedule
    decimal numberOfHours = (decimal)(newSchedule.EndTime - newSchedule.StartTime).TotalHours;
    decimal totalHoursAfterAdding =
      existingSchedules.Sum(cs => (decimal)(cs.EndTime - cs.StartTime).TotalHours) + numberOfHours;
    decimal expectedTotalHours = daysPerWeek * hoursPerDay;

    // When adding the last schedule, total hours must equal expected total
    if (existingSchedules.Count + 1 == daysPerWeek &&
        Math.Abs(totalHoursAfterAdding - expectedTotalHours) > HourTolerance)
      return ClassScheduleValidationResult.Failure(
        $"Total hours ({totalHoursAfterAdding:F2}) must equal days per week ({daysPerWeek}) × hours per day ({hoursPerDay}) = {expectedTotalHours:F2} hours.");

    // Cannot add more than DaysPerWeek schedules
    if (existingSchedules.Count(s => s.IsActive) + 1 > daysPerWeek)
      return ClassScheduleValidationResult.Failure(
        $"Cannot add more than {daysPerWeek} schedule(s) per week. Current count: {existingSchedules.Count}.");

    // Schedule hours must not exceed HoursPerDay
    if (numberOfHours - hoursPerDay > HourTolerance)
      return ClassScheduleValidationResult.Failure(
        $"Schedule hours ({numberOfHours:F2}) must not exceed ({hoursPerDay}) × hours per day.");

    // Schedule hours must not be less than HoursPerDay
    if (hoursPerDay - numberOfHours > HourTolerance)
      return ClassScheduleValidationResult.Failure(
        $"Schedule hours ({numberOfHours:F2}) must not be less than ({hoursPerDay}) × hours per day.");

    return ClassScheduleValidationResult.Success();
  }

  /// <summary>
  /// Validates updating an existing class schedule.
  /// </summary>
  /// <param name="existingSchedules">Collection of existing class schedules</param>
  /// <param name="scheduleIdToUpdate">ID of the schedule being updated</param>
  /// <param name="newStartTime">New start time</param>
  /// <param name="newEndTime">New end time</param>
  /// <param name="daysPerWeek">Expected number of days per week</param>
  /// <param name="hoursPerDay">Expected number of hours per day</param>
  /// <returns>Validation result with error message if validation fails</returns>
  public static ClassScheduleValidationResult ValidateUpdateClassSchedule(
    IReadOnlyCollection<ClassSchedule> existingSchedules,
    ClassScheduleId scheduleIdToUpdate,
    TimeOnly newStartTime,
    TimeOnly newEndTime,
    int daysPerWeek,
    decimal hoursPerDay)
  {
    // Calculate new duration
    decimal newScheduleDuration = (decimal)(newEndTime - newStartTime).TotalHours;
    decimal totalHoursAfterUpdate = existingSchedules
      .Where(cs => cs.Id != scheduleIdToUpdate)
      .Sum(cs => (decimal)(cs.EndTime - cs.StartTime).TotalHours) + newScheduleDuration;

    decimal expectedTotalHours = daysPerWeek * hoursPerDay;

    // Total hours after update cannot exceed expected total
    if (totalHoursAfterUpdate - expectedTotalHours > HourTolerance)
      return ClassScheduleValidationResult.Failure(
        $"Total hours after update ({totalHoursAfterUpdate:F2}) must not exceed days per week ({daysPerWeek}) × hours per day ({hoursPerDay}) = {expectedTotalHours:F2} hours.");

    return ClassScheduleValidationResult.Success();
  }

  /// <summary>
  /// Validates that all required class schedules are present.
  /// </summary>
  /// <param name="existingSchedules">Collection of existing class schedules</param>
  /// <param name="daysPerWeek">Expected number of days per week</param>
  /// <returns>True if the number of schedules meets or exceeds the required days per week</returns>
  public static bool AreSchedulesSufficient(
    IReadOnlyCollection<ClassSchedule> existingSchedules,
    int daysPerWeek)
  {
    return existingSchedules.Count == daysPerWeek;
  }

  /// <summary>
  /// Validates all class schedules as a complete set against the requirements.
  /// Ensures all schedules exist, have correct duration, and meet the expected total hours.
  /// </summary>
  /// <param name="allSchedules">Collection of all class schedules</param>
  /// <param name="daysPerWeek">Expected number of days per week</param>
  /// <param name="hoursPerDay">Expected number of hours per day</param>
  /// <returns>Validation result with error message if validation fails</returns>
  public static ClassScheduleValidationResult ValidateAllSchedules(
    IReadOnlyCollection<ClassSchedule> allSchedules,
    int daysPerWeek,
    decimal hoursPerDay)
  {
    // Check if schedule count matches required days per week
    if (allSchedules.Count < daysPerWeek)
      return ClassScheduleValidationResult.Failure(
        $"Expected {daysPerWeek} schedule(s) but found {allSchedules.Count}. All required schedules must be defined.");

    if (allSchedules.Count > daysPerWeek)
      return ClassScheduleValidationResult.Failure(
        $"Cannot have more than {daysPerWeek} schedule(s) per week. Current count: {allSchedules.Count}.");

    // Validate each schedule has the correct duration
    foreach (ClassSchedule schedule in allSchedules)
    {
      decimal scheduleHours = (decimal)(schedule.EndTime - schedule.StartTime).TotalHours;

      // Each schedule must have exactly hoursPerDay hours
      if (Math.Abs(scheduleHours - hoursPerDay) > HourTolerance)
        return ClassScheduleValidationResult.Failure(
          $"Schedule ({schedule.DayOfWeek}: {schedule.StartTime} - {schedule.EndTime}) has {scheduleHours:F2} hours but must have exactly {hoursPerDay} hours.");
    }

    // Validate total hours
    decimal totalScheduleHours = allSchedules.Sum(cs => (decimal)(cs.EndTime - cs.StartTime).TotalHours);
    decimal expectedTotalHours = daysPerWeek * hoursPerDay;

    if (Math.Abs(totalScheduleHours - expectedTotalHours) > HourTolerance)
      return ClassScheduleValidationResult.Failure(
        $"Total hours ({totalScheduleHours:F2}) must equal {daysPerWeek} days × {hoursPerDay} hours/day = {expectedTotalHours:F2} hours.");

    return ClassScheduleValidationResult.Success();
  }
}
