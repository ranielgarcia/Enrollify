using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionSchedulingStatsAggregateTypeEnum : SmartEnum<ClassSectionSchedulingStatsAggregateTypeEnum>
{
  public ClassSectionValidationIssueTierEnum? IssueTier { get; private set; }

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DraftSections = new("Draft Sections", 1);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum OpenSections = new("Open Sections", 2);
  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum CancelledSections = new("Cancelled Sections", 3);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum HardConflictIssues =
    new("Hard Conflict Issues", 4, ClassSectionValidationIssueTierEnum.ConflictHard);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum SoftConflictIssues =
    new("Soft Conflict Issues", 5, ClassSectionValidationIssueTierEnum.ConflictSoft);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum DataIntegrityIssue =
    new("Data Integrity Issue", 6, ClassSectionValidationIssueTierEnum.DataIntegrity);

  public static readonly ClassSectionSchedulingStatsAggregateTypeEnum Informational =
    new("Informational", 7, ClassSectionValidationIssueTierEnum.Informational);

  public ClassSectionSchedulingStatsAggregateTypeEnum(string name, int value,
    ClassSectionValidationIssueTierEnum? issueTier = null) : base(name, value)
  {
    IssueTier = issueTier;
  }
}
