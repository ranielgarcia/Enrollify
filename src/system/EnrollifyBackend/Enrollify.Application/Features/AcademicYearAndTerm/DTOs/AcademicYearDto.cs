using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.AcademicYearAndTerm.DTOs;

public class AcademicYearDto : BaseDto
{
    public AcademicYearId Id { get; private set; }

    public AcademicYearStartDate StartDate { get; private set; }
    public AcademicYearEndDate EndDate { get; private set; }

    public Year StartYear { get; private set; }
    public Year EndYear { get; private set; }

    public string AcademicYearTitle { get; private set; } = string.Empty;
    public string AcademicYearSlug { get; private set; } = string.Empty;

    public AcademicTermDto[] AcademicTerms { get; private set; } = Array.Empty<AcademicTermDto>();

    public static AcademicYearDto FromEntity(AcademicYear entity)
    {
        return new AcademicYearDto
        {
            Id = entity.Id,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            StartYear = entity.StartYear,
            EndYear = entity.EndYear,
            AcademicYearTitle = entity.AcademicYearTitle,
            AcademicYearSlug = entity.AcademicYearSlug,
            AcademicTerms = entity.AcademicTerms.Select(AcademicTermDto.FromEntity).ToArray(),
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(entity.CreatedByUser),
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(entity.UpdatedByUser),
            IsActive = entity.IsActive
        };
    }
}
