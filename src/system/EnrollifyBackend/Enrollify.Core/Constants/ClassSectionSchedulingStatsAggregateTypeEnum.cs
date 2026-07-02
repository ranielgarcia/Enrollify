using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionSchedulingStatsAggregateTypeEnum : SmartEnum<ClassSectionSchedulingStatsAggregateTypeEnum>
{
  public ClassSectionValidationIssueCategoryEnum? IssueCategory { get; private set; }

  // College Level Stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DRAFT_SECTIONS = new("DRAFT_SECTIONS", 1);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OPEN_SECTIONS = new("OPEN_SECTIONS", 2);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum CANCELLED_SECTIONS = new("CANCELLED_SECTIONS", 3);

  // Section-level category stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum SCHEDULE_CONFLICTS =
    new("SCHEDULE_CONFLICTS", 4, ClassSectionValidationIssueCategoryEnum.SCHEDULE_CONFLICT);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum SCHEDULE_POLICY_VIOLATIONS =
    new("SCHEDULE_POLICY_VIOLATIONS", 5, ClassSectionValidationIssueCategoryEnum.SCHEDULE_POLICY_VIOLATION);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum CAPACITY_CONSTRAINTS =
    new("CAPACITY_CONSTRAINTS", 6, ClassSectionValidationIssueCategoryEnum.CAPACITY_CONSTRAINT);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum RESOURCE_MISALIGNMENTS =
    new("RESOURCE_MISALIGNMENTS", 7, ClassSectionValidationIssueCategoryEnum.RESOURCE_MISALIGNMENT);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum MISSING_REQUIREMENTS =
    new("MISSING_REQUIREMENTS", 8, ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DATA_INCONSISTENCIES =
    new("DATA_INCONSISTENCIES", 9, ClassSectionValidationIssueCategoryEnum.DATA_INCONSISTENCY);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DEFAULT_VALUES =
    new("DEFAULT_VALUES", 10, ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE);

  // Class Level Stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_WITH_ISSUE_COUNT =
    new("OFFERING_WITH_ISSUE_COUNT", 11);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_MISSING_TEACHER_COUNT =
    new("OFFERING_MISSING_TEACHER_COUNT", 12);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_MISSING_ROOM_COUNT =
    new("OFFERING_MISSING_ROOM_COUNT", 13);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERING_NO_SCHEDULE_COUNT =
    new("OFFERING_NO_SCHEDULE_COUNT", 14);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OFFERINGS_COUNT =
    new("OFFERINGS_COUNT", 15);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum TOTAL_VALIDATION_ISSUES_ACROSS_OFFERINGS_COUNT =
    new("TOTAL_VALIDATION_ISSUES_ACROSS_OFFERINGS_COUNT", 16);

  public ClassSectionSchedulingStatsAggregateTypeEnum(string name, int value,
    ClassSectionValidationIssueCategoryEnum? issueCategory = null) : base(name, value)
  {
    IssueCategory = issueCategory;
  }
}
