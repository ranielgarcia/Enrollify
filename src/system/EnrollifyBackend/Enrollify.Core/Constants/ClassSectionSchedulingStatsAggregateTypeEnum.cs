using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionSchedulingStatsAggregateTypeEnum : SmartEnum<ClassSectionSchedulingStatsAggregateTypeEnum>
{
  public ClassSectionValidationIssueTierEnum? IssueTier { get; private set; }

  // College Level Stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DRAFT_SECTIONS = new("DRAFT_SECTIONS", 1);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OPEN_SECTIONS = new("OPEN_SECTIONS", 2);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum CANCELLED_SECTIONS = new("CANCELLED_SECTIONS", 3);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum HARD_CONFLICTS =
    new("HARD_CONFLICTS", 4, ClassSectionValidationIssueTierEnum.CONFLICT_HARD);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum SOFT_CONFLICTS =
    new("SOFT_CONFLICTS", 5, ClassSectionValidationIssueTierEnum.CONFLICT_SOFT);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DATA_INTEGRITY =
    new("DATA_INTEGRITY", 6, ClassSectionValidationIssueTierEnum.DATA_INTEGRITY);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum INFORMATIONAL =
    new("INFORMATIONAL", 7, ClassSectionValidationIssueTierEnum.INFORMATIONAL);

  // Class Level Stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_WITH_ISSUE_COUNT =
    new("OFFERING_WITH_ISSUE_COUNT", 8);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_MISSING_TEACHER_COUNT =
    new("OFFERING_MISSING_TEACHER_COUNT", 9);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_MISSING_ROOM_COUNT =
    new("OFFERING_MISSING_ROOM_COUNT", 10);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_NO_SCHEDULE_COUNT =
    new("OFFERING_NO_SCHEDULE_COUNT", 11);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERINGS_COUNT =
    new("OFFERINGS_COUNT", 12);

  public ClassSectionSchedulingStatsAggregateTypeEnum(string name, int value,
    ClassSectionValidationIssueTierEnum? issueTier = null) : base(name, value)
  {
    IssueTier = issueTier;
  }
}
