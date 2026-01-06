using Enrollify.Core.Constants;

namespace Enrollify.Core.Models;

public class CurriculaSettings
{
    public static readonly string Key = nameof (CurriculaSettings);
    public AcademicSystemEnum AcademicSystem { get; set; } = AcademicSystemEnum.Semester;
}
