using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionValidationIssueTierEnum : SmartEnum<ClassSectionValidationIssueTierEnum>
{
  public static readonly ClassSectionValidationIssueTierEnum CONFLICT_HARD = new("CONFLICT_HARD", 1);
  public static readonly ClassSectionValidationIssueTierEnum CONFLICT_SOFT = new("CONFLICT_SOFT", 2);
  public static readonly ClassSectionValidationIssueTierEnum DATA_INTEGRITY = new("DATA_INTEGRITY", 3);
  public static readonly ClassSectionValidationIssueTierEnum INFORMATIONAL = new("INFORMATIONAL", 4);

  public ClassSectionValidationIssueTierEnum(string name, int value) : base(name, value)
  {
  }
}
