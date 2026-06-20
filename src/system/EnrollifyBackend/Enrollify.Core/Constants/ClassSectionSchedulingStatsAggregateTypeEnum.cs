using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionSchedulingStatsAggregateTypeEnum : SmartEnum<ClassSectionSchedulingStatsAggregateTypeEnum>
{
  public ClassSectionValidationIssueTierEnum? IssueTier { get; private set; }

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

  public ClassSectionSchedulingStatsAggregateTypeEnum(string name, int value,
    ClassSectionValidationIssueTierEnum? issueTier = null) : base(name, value)
  {
    IssueTier = issueTier;
  }
}
