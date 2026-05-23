using Enrollify.Core.Constants;
using Enrollify.Core.Extensions;

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

    /// <summary>
    /// The maximum allowable year level
    /// This will be use when creating new class sections, etc.
    /// But changing its value should not affect the existing data.
    /// </summary>
    public int MaximumAllowableYearLevel { get; private set; } = 4;

    public YearLevelOption[] YearLevelOptions
    {
        get
        {
            return Enumerable.Range(1, MaximumAllowableYearLevel).Select(i => new YearLevelOption(i, i.ToOrdinal())).ToArray();
        }
    }

    public record YearLevelOption(int value, string label);   
}
