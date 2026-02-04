using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Curriculums.DTOs;

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

    public static CurriculumDetailDTO FromEntity(Curriculum curriculum)
    {
        return new CurriculumDetailDTO
        {
            Id = curriculum.Id,
            EffectiveYear = curriculum.EffectiveYear,
            Version = curriculum.Version,
            Status = curriculum.StatusId,
            Course = curriculum.Course != null ? CourseSummaryDTO.FromEntity(curriculum.Course) : null,
            Description = curriculum.Description,
            ApprovedDate = curriculum.ApprovedDate,
            CurriculumSubjects = curriculum.CurriculumSubjects
                .Where(cs => cs.IsActive)
                .Select(CurriculumSubjectDTO.FromEntity)
                .ToList(),
        };
    }
}
