using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionValidationIssueTierEnum : SmartEnum<ClassSectionValidationIssueTierEnum>
{
  public static readonly ClassSectionValidationIssueTierEnum ConflictHard = new("Conflict Hard", 1);
  public static readonly ClassSectionValidationIssueTierEnum ConflictSoft = new("Conflict Soft", 2);
  public static readonly ClassSectionValidationIssueTierEnum DataIntegrity = new("Data Integrity", 3);
  public static readonly ClassSectionValidationIssueTierEnum Informational = new("Informational", 4);

  public ClassSectionValidationIssueTierEnum(string name, int value) : base(name, value)
  {
  }
}
