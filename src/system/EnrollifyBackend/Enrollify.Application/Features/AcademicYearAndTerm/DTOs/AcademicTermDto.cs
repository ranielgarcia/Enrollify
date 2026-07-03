using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.AcademicYearAndTerm.DTOs;

public class AcademicTermDto : BaseDto
{
    public AcademicTermId Id { get; private set; }
    public TermNumber TermNumber { get; private set; }
    public string TermName { get; private set; }
    public AcademicYearId AcademicYearId { get; private set; }
    public AcademicTermStartDate StartDate { get; private set; }
    public AcademicTermEndDate EndDate { get; private set; }

    public static AcademicTermDto FromEntity(AcademicTerm entity)
    {
        return new AcademicTermDto
        {
            Id = entity.Id,
            TermNumber = entity.TermNumber,
            TermName = entity.TermName,
            AcademicYearId = entity.AcademicYearId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            CreatedAt = entity.CreatedAt,
            CreatedBy = BaseUserDto.FromUser(entity.CreatedByUser),
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = BaseUserDto.FromUser(entity.UpdatedByUser),
            IsActive = entity.IsActive
        };
    }
}
