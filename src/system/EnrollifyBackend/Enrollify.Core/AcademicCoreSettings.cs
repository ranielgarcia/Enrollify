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
    
    /// <summary>
    /// Minimum break time (in minutes) required between consecutive classes for the same teacher.
    /// Used for SC-03 (TEACHER_NO_BREAK) conflict detection in Phase 3.
    /// Default: 10 minutes.
    /// </summary>
    public int MinimumTeacherBreakMinutes { get; private set; } = 10;
    
    /// <summary>
    /// Standard operating hours for class schedules.
    /// Classes scheduled outside this range will trigger SC-06 (OUTSIDE_OPERATING_HOURS) warning in Phase 3.
    /// Default: 07:00 - 21:00
    /// </summary>
    public TimeOnly EarliestClassStartTime { get; private set; } = new TimeOnly(7, 0);
    public TimeOnly LatestClassEndTime { get; private set; } = new TimeOnly(21, 0);
}
