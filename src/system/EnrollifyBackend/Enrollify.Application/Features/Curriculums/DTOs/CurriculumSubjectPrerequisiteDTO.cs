using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class CurriculumSubjectPrerequisiteDTO
{
    public CurriculumSubjectId PrerequisiteCurriculumSubjectId { get; set; }
    public decimal? MinimumGrade { get; set; }
}
