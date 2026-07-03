using Enrollify.Application.SharedDTOs;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class CurriculumDetailDto
{
    public CurriculumId Id { get; set; }

    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;

    public CurriculumStatusEnum Status { get; set; } = null!;

    public CourseSummaryDto? Course { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset? ApprovedDate { get; set; }

    public IReadOnlyCollection<CurriculumSubjectDto> CurriculumSubjects { get; set; } = [];
}
