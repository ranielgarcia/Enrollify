using Enrollify.Core.Constants;

namespace Enrollify.Core;

public class AcademicCoreSettings
{
    /// <summary>
    /// The academic system must be hard-coded and should not be modified after initial configuration.
    /// Changing this value will require a complete data migration to ensure consistency across all
    /// academic records, schedules, and enrollment data. This property defines the institutional
    /// calendar structure (e.g., Trimester, Semester, Quarter) that affects all academic operations.
    /// </summary>
    public int AcademicTermSystem { get; private set; } = AcademicTermSystems.Trimester;
}
