using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassScheduleConflictTypeEnum : SmartEnum<ClassScheduleConflictTypeEnum>
{
  public ClassSectionConflictTierEnum Tier { get; set; }

  public static readonly ClassScheduleConflictTypeEnum TEACHER_DOUBLE_BOOKED = new("TEACHER_DOUBLE_BOOKED", 1, ClassSectionConflictTierEnum.Hard);
  public static readonly ClassScheduleConflictTypeEnum ROOM_DOUBLE_BOOKED = new("ROOM_DOUBLE_BOOKED", 2, ClassSectionConflictTierEnum.Hard);
  public static readonly ClassScheduleConflictTypeEnum SECTION_OVERLAP = new("SECTION_OVERLAP", 3, ClassSectionConflictTierEnum.Hard);
  public static readonly ClassScheduleConflictTypeEnum DUPLICATE_DAY_IN_OFFERING = new("DUPLICATE_DAY_IN_OFFERING", 4, ClassSectionConflictTierEnum.Hard);


  public static readonly ClassScheduleConflictTypeEnum TEACHER_OVERLOAD = new("TEACHER_OVERLOAD", 5, ClassSectionConflictTierEnum.Soft);
  public static readonly ClassScheduleConflictTypeEnum ROOM_CAPACITY_EXCEEDED = new("ROOM_CAPACITY_EXCEEDED", 6, ClassSectionConflictTierEnum.Soft);
  public static readonly ClassScheduleConflictTypeEnum TEACHER_NO_BREAK = new("TEACHER_NO_BREAK", 7, ClassSectionConflictTierEnum.Soft);
  public static readonly ClassScheduleConflictTypeEnum ADVISER_AS_TEACHER = new("ADVISER_AS_TEACHER", 8, ClassSectionConflictTierEnum.Soft);
  public static readonly ClassScheduleConflictTypeEnum YEAR_LEVEL_MISMATCH = new("YEAR_LEVEL_MISMATCH", 9, ClassSectionConflictTierEnum.Soft);
  public static readonly ClassScheduleConflictTypeEnum OUTSIDE_OPERATING_HOURS = new("OUTSIDE_OPERATING_HOURS", 10, ClassSectionConflictTierEnum.Soft);

  public static readonly ClassScheduleConflictTypeEnum SCHEDULE_COUNT_MISMATCH = new("SCHEDULE_COUNT_MISMATCH", 11, ClassSectionConflictTierEnum.DataIntegrity);
  public static readonly ClassScheduleConflictTypeEnum HOURS_MISMATCH = new("HOURS_MISMATCH", 12, ClassSectionConflictTierEnum.DataIntegrity);
  public static readonly ClassScheduleConflictTypeEnum NO_SCHEDULES = new("NO_SCHEDULES", 13, ClassSectionConflictTierEnum.DataIntegrity);
  public static readonly ClassScheduleConflictTypeEnum NO_OFFERINGS = new("NO_OFFERINGS", 14, ClassSectionConflictTierEnum.DataIntegrity);
  public static readonly ClassScheduleConflictTypeEnum DUPLICATE_SUBJECT_IN_SECTION = new("DUPLICATE_SUBJECT_IN_SECTION", 15, ClassSectionConflictTierEnum.DataIntegrity);

  public static readonly ClassScheduleConflictTypeEnum ROOM_TYPE_MISMATCH = new("ROOM_TYPE_MISMATCH", 16, ClassSectionConflictTierEnum.Informational);
  public static readonly ClassScheduleConflictTypeEnum CROSS_TERM_BOOKING = new("CROSS_TERM_BOOKING", 17, ClassSectionConflictTierEnum.Informational);

  public ClassScheduleConflictTypeEnum(string name, int value, ClassSectionConflictTierEnum tier) : base(name, value)
  {
    Tier = tier;
  }
}
