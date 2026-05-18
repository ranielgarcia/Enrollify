using Enrollify.Application.SharedDTOs;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.Curriculums.DTOs;

public class CurriculumDto : BaseDto
{
    public CurriculumId Id { get; set; }

    public int EffectiveYear { get; set; }
    public string Version { get; set; } = null!;
    public CurriculumStatusEnum Status { get; set; } = null!;
    public CourseSummaryDto? Course { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ApprovedDate { get; set; }

    public static CurriculumDto FromEntity(Curriculum curriculum)
    {
        return new CurriculumDto
        {
            Id = curriculum.Id,
            EffectiveYear = curriculum.EffectiveYear.Value,
            Version = curriculum.Version,
            Status = curriculum.StatusId,
            Course = curriculum.Course != null ? CourseSummaryDto.FromEntity(curriculum.Course) : null,
            Description = curriculum.Description,
            ApprovedDate = curriculum.ApprovedDate,
            CreatedAt = curriculum.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(curriculum.CreatedByUser),
            UpdatedAt = curriculum.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(curriculum.UpdatedByUser),
            IsActive = curriculum.IsActive,
        };
    }
}
