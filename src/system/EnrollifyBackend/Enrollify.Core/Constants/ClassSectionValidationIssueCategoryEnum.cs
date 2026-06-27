using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionValidationIssueCategoryEnum : SmartEnum<ClassSectionValidationIssueCategoryEnum>
{
  public static readonly ClassSectionValidationIssueCategoryEnum SCHEDULE_CONFLICT =
    new("SCHEDULE_CONFLICT", 1);

  public static readonly ClassSectionValidationIssueCategoryEnum SCHEDULE_POLICY_VIOLATION =
    new("SCHEDULE_POLICY_VIOLATION", 2);

  public static readonly ClassSectionValidationIssueCategoryEnum CAPACITY_CONSTRAINT =
    new("CAPACITY_CONSTRAINT", 3);

  public static readonly ClassSectionValidationIssueCategoryEnum RESOURCE_MISALIGNMENT =
    new("RESOURCE_MISALIGNMENT", 4);

  public static readonly ClassSectionValidationIssueCategoryEnum MISSING_REQUIREMENT =
    new("MISSING_REQUIREMENT", 5);

  public static readonly ClassSectionValidationIssueCategoryEnum DATA_INCONSISTENCY =
    new("DATA_INCONSISTENCY", 6);

  public static readonly ClassSectionValidationIssueCategoryEnum DEFAULT_VALUE =
    new("DEFAULT_VALUE", 7);

  public ClassSectionValidationIssueCategoryEnum(string name, int value) : base(name, value)
  {
  }
}
