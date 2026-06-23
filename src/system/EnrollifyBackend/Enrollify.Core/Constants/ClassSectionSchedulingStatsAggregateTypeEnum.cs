using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionSchedulingStatsAggregateTypeEnum : SmartEnum<ClassSectionSchedulingStatsAggregateTypeEnum>
{
  public ClassSectionValidationIssueTierEnum? IssueTier { get; private set; }

  // College Level Stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DraftSections = new("DRAFT_SECTIONS", 1);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OpenSections = new("OPEN_SECTIONS", 2);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum CancelledSections = new("CANCELLED_SECTIONS", 3);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum HardConflictIssues =
    new("HARD_CONFLICTS", 4, ClassSectionValidationIssueTierEnum.ConflictHard);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum SoftConflictIssues =
    new("SOFT_CONFLICTS", 5, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DataIntegrityIssue =
    new("DATA_INTEGRITY", 6, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum Informational =
    new("INFORMATIONAL", 7, ClassSectionValidationIssueTierEnum.Informational);

  // Class Level Stats
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OfferingWithIssueCount =
    new("OFFERING_WITH_ISSUE_COUNT", 8);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OfferingMissingTeacherCount =
    new("OFFERING_MISSING_TEACHER_COUNT", 9);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OfferingMissingRoomCount =
    new("OFFERING_MISSING_ROOM_COUNT", 10);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OfferingNoScheduleCount =
    new("OFFERING_NO_SCHEDULE_COUNT", 11);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OfferingsCount =
    new("OFFERINGS_COUNT", 12);

  public ClassSectionSchedulingStatsAggregateTypeEnum(string name, int value,
    ClassSectionValidationIssueTierEnum? issueTier = null) : base(name, value)
  {
    IssueTier = issueTier;
  }
}
