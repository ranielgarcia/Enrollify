namespace Enrollify.Application.Features.AcademicYearAndTerm.DTOs;

public class AcademicYearContextDto
{
    public List<AcademicYearDto> Previous { get; set; } = new List<AcademicYearDto>();
    public AcademicYearDto? Current { get; set; }
    public List<AcademicYearDto> Future { get; set; } = new List<AcademicYearDto>();
}
