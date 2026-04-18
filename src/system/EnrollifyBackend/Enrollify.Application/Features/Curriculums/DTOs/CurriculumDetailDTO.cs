using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class CurriculumDetailDTO
{
    public CurriculumId Id { get; set; }

    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;

    public CurriculumStatusEnum Status { get; set; } = null!;

    public CourseSummaryDTO? Course { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset? ApprovedDate { get; set; }

    public IReadOnlyCollection<CurriculumSubjectDTO> CurriculumSubjects { get; set; } = [];
}
