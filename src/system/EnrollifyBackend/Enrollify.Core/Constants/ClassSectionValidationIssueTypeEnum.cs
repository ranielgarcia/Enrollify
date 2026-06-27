using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionValidationIssueTypeEnum : SmartEnum<ClassSectionValidationIssueTypeEnum>
{
  public ClassSectionValidationIssueCategoryEnum Category { get; set; }

  // SCHEDULE_CONFLICT — direct time/resource booking conflicts
  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_DOUBLE_BOOKED =
    new("TEACHER_DOUBLE_BOOKED", 1, ClassSectionValidationIssueCategoryEnum.SCHEDULE_CONFLICT);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_DOUBLE_BOOKED =
    new("ROOM_DOUBLE_BOOKED", 2, ClassSectionValidationIssueCategoryEnum.SCHEDULE_CONFLICT);

  public static readonly ClassSectionValidationIssueTypeEnum SECTION_OVERLAP =
    new("SECTION_OVERLAP", 3, ClassSectionValidationIssueCategoryEnum.SCHEDULE_CONFLICT);

  // SCHEDULE_POLICY_VIOLATION — schedule configuration breaks rules
  public static readonly ClassSectionValidationIssueTypeEnum DUPLICATE_DAY_IN_OFFERING =
    new("DUPLICATE_DAY_IN_OFFERING", 4, ClassSectionValidationIssueCategoryEnum.SCHEDULE_POLICY_VIOLATION);

  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_NO_BREAK =
    new("TEACHER_NO_BREAK", 7, ClassSectionValidationIssueCategoryEnum.SCHEDULE_POLICY_VIOLATION);

  public static readonly ClassSectionValidationIssueTypeEnum OUTSIDE_OPERATING_HOURS =
    new("OUTSIDE_OPERATING_HOURS", 10, ClassSectionValidationIssueCategoryEnum.SCHEDULE_POLICY_VIOLATION);

  public static readonly ClassSectionValidationIssueTypeEnum CROSS_TERM_BOOKING =
    new("CROSS_TERM_BOOKING", 20, ClassSectionValidationIssueCategoryEnum.SCHEDULE_POLICY_VIOLATION);

  // CAPACITY_CONSTRAINT — resource capacity/load exceeded
  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_OVERLOAD =
    new("TEACHER_OVERLOAD", 5, ClassSectionValidationIssueCategoryEnum.CAPACITY_CONSTRAINT);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_CAPACITY_EXCEEDED =
    new("ROOM_CAPACITY_EXCEEDED", 6, ClassSectionValidationIssueCategoryEnum.CAPACITY_CONSTRAINT);

  // RESOURCE_MISALIGNMENT — assigned resource doesn't fit context
  public static readonly ClassSectionValidationIssueTypeEnum ADVISER_AS_TEACHER =
    new("ADVISER_AS_TEACHER", 8, ClassSectionValidationIssueCategoryEnum.RESOURCE_MISALIGNMENT);

  public static readonly ClassSectionValidationIssueTypeEnum YEAR_LEVEL_MISMATCH =
    new("YEAR_LEVEL_MISMATCH", 9, ClassSectionValidationIssueCategoryEnum.RESOURCE_MISALIGNMENT);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_TYPE_MISMATCH =
    new("ROOM_TYPE_MISMATCH", 19, ClassSectionValidationIssueCategoryEnum.RESOURCE_MISALIGNMENT);

  // MISSING_REQUIREMENT — essential data absent
  public static readonly ClassSectionValidationIssueTypeEnum ADVISER_NOT_ASSIGNED =
    new("ADVISER_NOT_ASSIGNED", 15, ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT);

  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_NOT_ASSIGNED =
    new("TEACHER_NOT_ASSIGNED", 16, ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_NOT_ASSIGNED =
    new("ROOM_NOT_ASSIGNED", 17, ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT);

  public static readonly ClassSectionValidationIssueTypeEnum NO_SCHEDULES = new("NO_SCHEDULES", 13,
    ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT);

  public static readonly ClassSectionValidationIssueTypeEnum NO_OFFERINGS = new("NO_OFFERINGS", 14,
    ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT);

  // DATA_INCONSISTENCY — data values contradict each other
  public static readonly ClassSectionValidationIssueTypeEnum SCHEDULE_COUNT_MISMATCH =
    new("SCHEDULE_COUNT_MISMATCH", 11, ClassSectionValidationIssueCategoryEnum.DATA_INCONSISTENCY);

  public static readonly ClassSectionValidationIssueTypeEnum HOURS_MISMATCH =
    new("HOURS_MISMATCH", 12, ClassSectionValidationIssueCategoryEnum.DATA_INCONSISTENCY);

  public static readonly ClassSectionValidationIssueTypeEnum DUPLICATE_SUBJECT_IN_SECTION =
    new("DUPLICATE_SUBJECT_IN_SECTION", 18, ClassSectionValidationIssueCategoryEnum.DATA_INCONSISTENCY);

  // DEFAULT_VALUE — unconfigured defaults being used (informational)
  public static readonly ClassSectionValidationIssueTypeEnum DAYS_PER_WEEK_DEFAULT =
    new("DAYS_PER_WEEK_DEFAULT", 21, ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE);

  public static readonly ClassSectionValidationIssueTypeEnum HOURS_PER_DAY_DEFAULT =
    new("HOURS_PER_DAY_DEFAULT", 22, ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE);

  public static readonly ClassSectionValidationIssueTypeEnum MAX_STUDENTS_AT_DEFAULT =
    new("MAX_STUDENTS_AT_DEFAULT", 23, ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE);

  public ClassSectionValidationIssueTypeEnum(string name, int value, ClassSectionValidationIssueCategoryEnum category) :
    base(name, value)
  {
    Category = category;
  }
}
