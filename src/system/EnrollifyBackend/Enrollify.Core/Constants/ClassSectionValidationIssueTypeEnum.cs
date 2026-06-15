using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionValidationIssueTypeEnum : SmartEnum<ClassSectionValidationIssueTypeEnum>
{
  public ClassSectionValidationIssueTierEnum Tier { get; set; }

  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_DOUBLE_BOOKED =
    new("TEACHER_DOUBLE_BOOKED", 1, ClassSectionValidationIssueTierEnum.ConflictHard);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_DOUBLE_BOOKED =
    new("ROOM_DOUBLE_BOOKED", 2, ClassSectionValidationIssueTierEnum.ConflictHard);

  public static readonly ClassSectionValidationIssueTypeEnum SECTION_OVERLAP =
    new("SECTION_OVERLAP", 3, ClassSectionValidationIssueTierEnum.ConflictHard);

  public static readonly ClassSectionValidationIssueTypeEnum DUPLICATE_DAY_IN_OFFERING =
    new("DUPLICATE_DAY_IN_OFFERING", 4, ClassSectionValidationIssueTierEnum.ConflictHard);


  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_OVERLOAD =
    new("TEACHER_OVERLOAD", 5, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_CAPACITY_EXCEEDED =
    new("ROOM_CAPACITY_EXCEEDED", 6, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_NO_BREAK =
    new("TEACHER_NO_BREAK", 7, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionValidationIssueTypeEnum ADVISER_AS_TEACHER =
    new("ADVISER_AS_TEACHER", 8, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionValidationIssueTypeEnum YEAR_LEVEL_MISMATCH =
    new("YEAR_LEVEL_MISMATCH", 9, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionValidationIssueTypeEnum OUTSIDE_OPERATING_HOURS =
    new("OUTSIDE_OPERATING_HOURS", 10, ClassSectionValidationIssueTierEnum.ConflictSoft);


  public static readonly ClassSectionValidationIssueTypeEnum SCHEDULE_COUNT_MISMATCH =
    new("SCHEDULE_COUNT_MISMATCH", 11, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum HOURS_MISMATCH =
    new("HOURS_MISMATCH", 12, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum NO_SCHEDULES = new("NO_SCHEDULES", 13,
    ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum NO_OFFERINGS = new("NO_OFFERINGS", 14,
    ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum ADVISER_NOT_ASSIGNED =
    new("ADVISER_NOT_ASSIGNED", 15, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum TEACHER_NOT_ASSIGNED =
    new("TEACHER_NOT_ASSIGNED", 16, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_NOT_ASSIGNED =
    new("ROOM_NOT_ASSIGNED", 17, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum DUPLICATE_SUBJECT_IN_SECTION =
    new("DUPLICATE_SUBJECT_IN_SECTION", 18, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionValidationIssueTypeEnum ROOM_TYPE_MISMATCH =
    new("ROOM_TYPE_MISMATCH", 19, ClassSectionValidationIssueTierEnum.Informational);

  public static readonly ClassSectionValidationIssueTypeEnum CROSS_TERM_BOOKING =
    new("CROSS_TERM_BOOKING", 20, ClassSectionValidationIssueTierEnum.Informational);

  public static readonly ClassSectionValidationIssueTypeEnum DAYS_PER_WEEK_DEFAULT =
    new("DAYS_PER_WEEK_DEFAULT", 21, ClassSectionValidationIssueTierEnum.Informational);

  public static readonly ClassSectionValidationIssueTypeEnum HOURS_PER_DAY_DEFAULT =
    new("HOURS_PER_DAY_DEFAULT", 22, ClassSectionValidationIssueTierEnum.Informational);

  public static readonly ClassSectionValidationIssueTypeEnum MAX_STUDENTS_AT_DEFAULT =
    new("MAX_STUDENTS_AT_DEFAULT", 23, ClassSectionValidationIssueTierEnum.Informational);

  public ClassSectionValidationIssueTypeEnum(string name, int value, ClassSectionValidationIssueTierEnum tier) :
    base(name, value)
  {
    Tier = tier;
  }
}
