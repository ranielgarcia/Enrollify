using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class CurriculumDTO : BaseDTO
{
    public CurriculumId Id { get; set; }

    public int EffectiveYear { get; set; }
    public string Version { get; set; } = null!;
    public CurriculumStatusEnum Status { get; set; } = null!;
    public CourseSummaryDTO? Course { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ApprovedDate { get; set; }

    public static CurriculumDTO FromEntity(Curriculum curriculum)
    {
        return new CurriculumDTO
        {
            Id = curriculum.Id,
            EffectiveYear = curriculum.EffectiveYear,
            Version = curriculum.Version,
            Status = curriculum.StatusId,
            Course = curriculum.Course != null ? CourseSummaryDTO.FromEntity(curriculum.Course) : null,
            Description = curriculum.Description,
            ApprovedDate = curriculum.ApprovedDate,
            CreatedAt = curriculum.CreatedAt,
            CreatedBy = BaseUserDTO.FromUser(curriculum.CreatedByUser),
            UpdatedAt = curriculum.UpdatedAt,
            UpdatedBy = BaseUserDTO.FromUser(curriculum.UpdatedByUser),
            IsActive = curriculum.IsActive,
        };
    }
}
