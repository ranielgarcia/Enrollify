using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public class ClassSectionConflictTierEnum : SmartEnum<ClassSectionConflictTierEnum>
{
  public static readonly ClassSectionConflictTierEnum Hard = new ClassSectionConflictTierEnum("Hard", 1);
  public static readonly ClassSectionConflictTierEnum Soft = new ClassSectionConflictTierEnum("Soft", 2);
  public static readonly ClassSectionConflictTierEnum DataIntegrity = new ClassSectionConflictTierEnum("DataIntegrity", 3);
  public static readonly ClassSectionConflictTierEnum Informational = new ClassSectionConflictTierEnum("Informational", 4);

  public ClassSectionConflictTierEnum(string name, int value) : base(name, value)
  {

  }
}
