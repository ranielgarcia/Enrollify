using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums.DTOs;

public class CurriculumSubjectPrerequisiteDTO
{
    public CurriculumSubjectId CurriculumSubjectId { get; set; }
    public CurriculumSubjectId PrerequisiteCurriculumSubjectId { get; set; }

    /// <summary>
    /// Optional minimum grade required (e.g., 2.0). Valid range: 1.0 to 5.0
    /// </summary>
    public decimal? MinimumGrade { get; set; }

    public static CurriculumSubjectPrerequisiteDTO FromEntity(CurriculumSubjectPrerequisite prerequisite)
    {
        return new CurriculumSubjectPrerequisiteDTO
        {
            CurriculumSubjectId = prerequisite.CurriculumSubjectId,
            PrerequisiteCurriculumSubjectId = prerequisite.PrerequisiteCurriculumSubjectId,
            MinimumGrade = prerequisite.MinimumGrade,
        };
    }
}
